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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7018, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });

    options.ListenAnyIP(5247, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

var payOsConfigs = PayOSConfigsLoader.LoadAndValidate(builder.Configuration);

builder.Services.AddSnowflakeIdGenerator(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
builder.Services.AddKeycloakJwtAuth(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history")));
builder.Services.AddTransient<BookingCreatedConsumer>();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentDomainService>();
builder.Services.AddSingleton(payOsConfigs);
builder.Services.AddSingleton(_ =>
    new PayOSClient(payOsConfigs.ClientId, payOsConfigs.ApiKey, payOsConfigs.ChecksumKey));

var app = builder.Build();
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();
    
app.UseMiddleware<HttpExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<GrpcService>();
app.MapControllers();

app.Run();
