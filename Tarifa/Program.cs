using KafkaFlow;
using KafkaFlow.Serializer;
using Tarifa.Domain.Interfaces;
using Tarifa.Infrastructure.Data;
using Tarifa.Infrastructure.Messaging;
using Tarifa.Infrastructure.Messaging.Consumers;
using Tarifa.Infrastructure.Messaging.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories
builder.Services.AddScoped<ITarifacaoRepository, TarifacaoRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// KafkaFlow
var kafkaBrokers = builder.Configuration.GetValue<string>("Kafka:Brokers") ?? "localhost:9092";

builder.Services.AddKafka(kafka => kafka
    .UseMicrosoftLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { kafkaBrokers })
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasRealizadas, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TarifacoesRealizadas, 1, 1)
        .AddProducer("tarifacao-realizada-producer", producer => producer
            .DefaultTopic(KafkaTopics.TarifacoesRealizadas)
            .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddConsumer(consumer => consumer
            .Topic(KafkaTopics.TransferenciasRealizadas)
            .WithGroupId("tarifa-consumer-group")
            .WithBufferSize(100)
            .WithWorkersCount(1)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .AddMiddlewares(middlewares => middlewares
                .AddSingleTypeDeserializer<TransferenciaRealizadaMessage, JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<TransferenciaRealizadaConsumer>()
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

// Endpoint de health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Tarifa" }))
    .WithName("HealthCheck")
    .WithOpenApi();

// Start Kafka
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();
