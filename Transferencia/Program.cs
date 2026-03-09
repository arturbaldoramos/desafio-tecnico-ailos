using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.OpenApi.Models;
using Transferencia.Domain.Interfaces;
using Transferencia.Infrastructure.Data;
using Transferencia.Infrastructure.Messaging;
using Transferencia.Infrastructure.Messaging.Consumers;
using Transferencia.Infrastructure.Messaging.Messages;
using Transferencia.Infrastructure.Security;
using Transferencia.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore - Transferencia API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Repositories
builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// HttpClient para comunicar com ContaCorrente API (para consultas de conta)
var contaCorrenteBaseUrl = builder.Configuration.GetValue<string>("ContaCorrenteApi:BaseUrl")
    ?? "http://localhost:5024";

builder.Services.AddHttpClient<IContaCorrenteApiClient, ContaCorrenteApiClient>(client =>
{
    client.BaseAddress = new Uri(contaCorrenteBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// KafkaFlow
var kafkaBrokers = builder.Configuration.GetValue<string>("Kafka:Brokers") ?? "localhost:9092";

builder.Services.AddKafka(kafka => kafka
    .UseMicrosoftLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { kafkaBrokers })
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasSolicitadas, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasResultado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasRealizadas, 1, 1)
        .AddProducer("transferencia-solicitada-producer", producer => producer
            .DefaultTopic(KafkaTopics.TransferenciasSolicitadas)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer("transferencia-realizada-producer", producer => producer
            .DefaultTopic(KafkaTopics.TransferenciasRealizadas)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.TransferenciasResultado)
            .WithGroupId("transferencia-resultado-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<TransferenciaResultadoMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<TransferenciaResultadoConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler: {context.Message}")
                    )
                )
            )
        )
    )
);

var app = builder.Build();

// Database Bootstrap
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    DatabaseBootstrap.Setup(config);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JwtClaimsMiddleware>();

app.MapControllers();

// Start Kafka
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();
