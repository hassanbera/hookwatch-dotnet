using App.Model;
using App.Repository;
using App.Common;
using static App.Common.Input;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace App.Service;

public class DomainService(IDomainRepository repo)
{

  public void Allow(string url) { if (!Uri.TryCreate(url, UriKind.Absolute, out var u) || !new[] { "http", "https" }.Contains(u.Scheme) || u.UserInfo != "" || u.Fragment != "") throw new ApiException(400, "Geçersiz hedef URL."); var defaults = Environment.GetEnvironmentVariable("APP_URLS") ?? "http://127.0.0.1:4206"; var allowed = (Environment.GetEnvironmentVariable("WEBHOOK_ALLOWED_ORIGINS") ?? defaults).Split(',').Select(s => s.Trim()); if (!allowed.Contains(u.GetLeftPart(UriPartial.Authority))) throw new ApiException(400, "Hedef origin izin listesinde değil."); }
  public async Task<object> Endpoints() => await repo.Query<Endpoint>().OrderBy(e => e.CreatedAt).ToListAsync();
  public async Task<object> Endpoint(JsonElement j) { var e = new Endpoint { Name = Text(j, "name", 100, true), Url = Text(j, "url", 500, true) }; Allow(e.Url); repo.Add(e); await repo.Save(); return e; }
  public async Task<object> Deliveries() => await repo.Query<Delivery>().OrderByDescending(d => d.CreatedAt).Take(100).ToListAsync();
  public async Task<object> Delivery(Guid id) => new { delivery = await repo.Find<Delivery>(id), attempts = await repo.Query<Attempt>().Where(a => a.DeliveryId == id).OrderBy(a => a.Number).ToListAsync() };
  public async Task<object> Create(JsonElement j) { var d = new Delivery { EndpointId = Id(j, "endpointId"), EventType = Text(j, "eventType", 80, true) }; await repo.Find<Endpoint>(d.EndpointId); if (!j.TryGetProperty("payload", out var p) || p.ValueKind != JsonValueKind.Object || p.GetRawText().Length > 32000) throw new ApiException(400, "JSON nesnesi en fazla 32 KB olmalı."); d.Payload = p.GetRawText(); repo.Add(d); await repo.Save(); return d; }
  public async Task<object> Retry(Guid id) { await using var tx = await repo.Begin(); var d = await repo.Lock<Delivery>(id); if (d.Status != "FAILED") throw new ApiException(409, "Yalnızca başarısız teslimat yeniden denenebilir."); d.Status = "QUEUED"; d.CycleAttempts = 0; d.NextAttemptAt = DateTimeOffset.UtcNow; await repo.Save(); await tx.CommitAsync(); return d; }

}
