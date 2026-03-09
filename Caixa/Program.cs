using Caixa.Domain.Interfaces;
using Caixa.Infrastructure.Data;
using Caixa.Infrastructure.Messaging;
using Caixa.Infrastructure.Messaging.Consumers;
using Caixa.Infrastructure.Messaging.Messages;
using Caixa.Infrastructure.Security;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore - Caixa API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
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
builder.Services.AddScoped<IDepositoRepository, DepositoRepository>();
builder.Services.AddScoped<ISaqueRepository, SaqueRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// KafkaFlow
var kafkaBrokers = builder.Configuration.GetValue<string>("Kafka:Brokers") ?? "localhost:9092";

builder.Services.AddKafka(kafka => kafka
    .UseMicrosoftLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { kafkaBrokers })
        .CreateTopicIfNotExists(KafkaTopics.DepositoSolicitado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.SaqueSolicitado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.DepositoResultado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.SaqueResultado, 1, 1)
        .AddProducer("deposito-solicitado-producer", producer => producer
            .DefaultTopic(KafkaTopics.DepositoSolicitado)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer("saque-solicitado-producer", producer => producer
            .DefaultTopic(KafkaTopics.SaqueSolicitado)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.DepositoResultado)
            .WithGroupId("caixa-deposito-resultado")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h
                    .AddHandler<DepositoResultadoConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler no tópico {context.ConsumerContext.Topic}"))
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.SaqueResultado)
            .WithGroupId("caixa-saque-resultado")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h
                    .AddHandler<SaqueResultadoConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler no tópico {context.ConsumerContext.Topic}"))
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
