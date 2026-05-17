using Common.Extensions;
using Common.Middleware;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PaymentService.Configs;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Extensions;
using PaymentService.Repositories;
using PaymentDomainService = PaymentService.Services.PaymentService;

var builder = WebApplication.CreateBuilder(args);
var payOsConfigs = PayOSConfigsLoader.LoadAndValidate(builder.Configuration);

builder.Services.AddSnowflakeIdGenerator(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddOpenApi();
builder.Services.AddKeycloakJwtAuth(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history")));
builder.Services.AddTransient<BookingCreatedConsumer>();
builder.Services.AddConsumerMessaging(builder.Configuration);
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentDomainService>();
builder.Services.AddSingleton(payOsConfigs);
builder.Services.AddSingleton(_ =>
    new PayOSClient(payOsConfigs.ClientId, payOsConfigs.ApiKey, payOsConfigs.ChecksumKey));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<HttpExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
