using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Cache;
using ContaCorrente.Infrastructure.Data;
using ContaCorrente.Infrastructure.Messaging;
using ContaCorrente.Infrastructure.Messaging.Consumers;
using ContaCorrente.Infrastructure.Messaging.Messages;
using ContaCorrente.Infrastructure.Security;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore - Conta Corrente API", Version = "v1" });

    // Configurar JWT no Swagger
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

// Cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISaldoCacheService, SaldoCacheService>();

// Repositories
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// KafkaFlow
var kafkaBrokers = builder.Configuration.GetValue<string>("Kafka:Brokers") ?? "localhost:9092";

builder.Services.AddKafka(kafka => kafka
    .UseMicrosoftLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { kafkaBrokers })
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasSolicitadas, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasResultado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TarifacoesRealizadas, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.UsuarioCadastrado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.UsuarioInativado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.DepositoSolicitado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.SaqueSolicitado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.DepositoResultado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.SaqueResultado, 1, 1)
        .AddProducer("transferencia-resultado-producer", producer => producer
            .DefaultTopic(KafkaTopics.TransferenciasResultado)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer("deposito-resultado-producer", producer => producer
            .DefaultTopic(KafkaTopics.DepositoResultado)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer("saque-resultado-producer", producer => producer
            .DefaultTopic(KafkaTopics.SaqueResultado)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.TransferenciasSolicitadas)
            .WithGroupId("contacorrente-transferencia-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<TransferenciaSolicitadaMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<TransferenciaSolicitadaConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler: {context.Message}")
                    )
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.TarifacoesRealizadas)
            .WithGroupId("contacorrente-tarifacao-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<TarifacaoRealizadaMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<TarifacaoRealizadaConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler: {context.Message}")
                    )
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.UsuarioCadastrado)
            .WithGroupId("contacorrente-usuario-cadastrado-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<UsuarioCadastradoMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<UsuarioCadastradoConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler: {context.Message}")
                    )
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.UsuarioInativado)
            .WithGroupId("contacorrente-usuario-inativado-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<UsuarioInativadoMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<UsuarioInativadoConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler: {context.Message}")
                    )
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.DepositoSolicitado)
            .WithGroupId("contacorrente-deposito-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<DepositoSolicitadoMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<DepositoSolicitadoConsumer>()
                    .WhenNoHandlerFound(context =>
                        Console.WriteLine($"Mensagem sem handler: {context.Message}")
                    )
                )
            )
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.SaqueSolicitado)
            .WithGroupId("contacorrente-saque-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<SaqueSolicitadoMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<SaqueSolicitadoConsumer>()
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
    BankMore.Infrastructure.Data.DatabaseBootstrap.Setup(config);
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
