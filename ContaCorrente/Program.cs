using System.Text;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Cache;
using ContaCorrente.Infrastructure.Data;
using ContaCorrente.Infrastructure.Messaging;
using ContaCorrente.Infrastructure.Messaging.Consumers;
using ContaCorrente.Infrastructure.Messaging.Messages;
using ContaCorrente.Infrastructure.Security;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings.GetValue<string>("SecretKey")!;
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
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

// Security Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();

// KafkaFlow
var kafkaBrokers = builder.Configuration.GetValue<string>("Kafka:Brokers") ?? "localhost:9092";

builder.Services.AddKafka(kafka => kafka
    .UseMicrosoftLog()
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { kafkaBrokers })
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasSolicitadas, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TransferenciasResultado, 1, 1)
        .CreateTopicIfNotExists(KafkaTopics.TarifacoesRealizadas, 1, 1)
        .AddProducer("transferencia-resultado-producer", producer => producer
            .DefaultTopic(KafkaTopics.TransferenciasResultado)
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Start Kafka
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();
