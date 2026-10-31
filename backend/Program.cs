using App.Repository;
using App.Service;
using App.Common;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("APP_URLS") ?? "http://127.0.0.1:4206");
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 65536);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "Host=localhost;Port=55506;Database=hookwatch;Username=app;Password=local-development-only"));
builder.Services.AddScoped<IDomainRepository, DomainRepository>(); builder.Services.AddScoped<DomainService>(); builder.Services.AddHostedService<DeliveryWorker>();
var app = builder.Build();
app.Use(async (ctx, next) => { try { await next(); } catch (Exception ex) { int code = ex is ApiException e ? e.Status : ex is DbUpdateException ? 409 : 500; app.Logger.Log(code == 500 ? LogLevel.Error : LogLevel.Debug, ex, "Request failed"); ctx.Response.StatusCode = code; await ctx.Response.WriteAsJsonAsync(new { error = ex is ApiException a ? a.Message : code == 409 ? "İşlem mevcut kayıtlarla çakışıyor." : "İşlem tamamlanamadı." }); } });
using (var scope = app.Services.CreateScope()) { await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync(); }
app.MapControllers(); await app.RunAsync();
