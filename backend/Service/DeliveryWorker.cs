using App.Model;
using App.Repository;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
namespace App.Service;

public class DeliveryWorker(IServiceScopeFactory scopes, ILogger<DeliveryWorker> logger) : BackgroundService
{
  private readonly HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(5) };
  protected override async Task ExecuteAsync(CancellationToken stoppingToken) { using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1)); while (await timer.WaitForNextTickAsync(stoppingToken)) { if (Environment.GetEnvironmentVariable("WORKER_ENABLED") == "false") continue; try { await Tick(stoppingToken); } catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; } catch (Exception e) { logger.LogError(e, "Worker cycle failed"); } } }
  private async Task Tick(CancellationToken ct)
  {
    using var scope = scopes.CreateScope(); var repo = scope.ServiceProvider.GetRequiredService<IDomainRepository>(); var service = scope.ServiceProvider.GetRequiredService<DomainService>(); await using var tx = await repo.Begin(); var d = await repo.Claim(); if (d == null) { await tx.CommitAsync(ct); return; }
    var a = new Attempt { DeliveryId = d.Id, Number = ++d.AttemptCount }; d.CycleAttempts++; var watch = Stopwatch.StartNew(); bool retry = true;
    try { var e = await repo.Find<Endpoint>(d.EndpointId); service.Allow(e.Url); var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(); var secret = Environment.GetEnvironmentVariable("WEBHOOK_SIGNING_SECRET") ?? "local-signing-key-change-before-deploy"; var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(ts + "." + d.Payload)); using var req = new HttpRequestMessage(HttpMethod.Post, e.Url) { Content = new StringContent(d.Payload, Encoding.UTF8, "application/json") }; req.Headers.Add("X-HookWatch-Id", d.Id.ToString()); req.Headers.Add("X-HookWatch-Timestamp", ts); req.Headers.Add("X-HookWatch-Signature", "sha256=" + Convert.ToHexString(hash).ToLowerInvariant()); using var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct); int code = (int)res.StatusCode; a.StatusCode = code; if (code >= 200 && code < 300) d.Status = "DELIVERED"; else { a.Error = "HTTP " + code; retry = code == 408 || code == 429 || code >= 500; } }
    catch (Exception e) when (!ct.IsCancellationRequested) { a.Error = "Gönderim başarısız: " + e.GetType().Name; }
    a.DurationMs = (int)watch.ElapsedMilliseconds; repo.Add(a); if (d.Status != "DELIVERED") { if (retry && d.CycleAttempts < 4) { d.Status = "QUEUED"; d.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(1 << d.CycleAttempts); } else d.Status = "FAILED"; }
    await repo.Save(); await tx.CommitAsync(ct);
  }
  public override void Dispose() { client.Dispose(); base.Dispose(); }
}
