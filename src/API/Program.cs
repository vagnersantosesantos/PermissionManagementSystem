using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Nest;
using PermissionManagement.API.Middleware;
using PermissionManagement.Application.Interfaces;
using PermissionManagement.Domain.Interfaces;
using PermissionManagement.Infrastructure.Data;
using PermissionManagement.Infrastructure.Repositories;
using PermissionManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Permission Management API", Version = "v1" });
});

// Database configuration
builder.Services.AddDbContext<PermissionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PermissionManagement.Application.Commands.RequestPermissionCommand).Assembly));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Elasticsearch configuration
var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(elasticsearchUrl))
    .DefaultIndex("permissions")
    .EnableApiVersioningHeader();

builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

// Kafka configuration
try
{
    var kafkaConfig = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
        MessageTimeoutMs = 3000,
        RequestTimeoutMs = 3000
    };

    var producer = new ProducerBuilder<string, string>(kafkaConfig).Build();
    builder.Services.AddSingleton<IProducer<string, string>>(provider => producer);
    builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();

    Console.WriteLine("✅ Kafka configurado com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Kafka não está disponível. A aplicação funcionará sem Kafka. Erro: {ex.Message}");

    builder.Services.AddSingleton<IProducer<string, string>>(provider => null!);
    builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Permission Management API v1");
        c.RoutePrefix = string.Empty;

    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();