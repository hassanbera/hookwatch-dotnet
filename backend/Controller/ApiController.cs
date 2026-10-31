using Microsoft.AspNetCore.Mvc;
using App.Service;
using App.Repository;
using System.Text.Json;
namespace App.Controller;

[ApiController]
[Route("api")]
public class ApiController(DomainService service, IDomainRepository repo) : ControllerBase
{
  [HttpGet("health")] public async Task<object> Health() { if (!await repo.Healthy()) throw new Exception("Database unavailable"); return new { data = new { status = "ok" } }; }
  [HttpGet("endpoints")] public async Task<object> Action0() { return new { data = await service.Endpoints() }; }
  [HttpPost("endpoints")] public async Task<object> Action1([FromBody] JsonElement j) { return new { data = await service.Endpoint(j) }; }
  [HttpGet("deliveries")] public async Task<object> Action2() { return new { data = await service.Deliveries() }; }
  [HttpPost("deliveries")] public async Task<object> Action3([FromBody] JsonElement j) { return new { data = await service.Create(j) }; }
  [HttpGet("deliveries/{id}")] public async Task<object> Action4(Guid id) { return new { data = await service.Delivery(id) }; }
  [HttpPost("deliveries/{id}/retry")] public async Task<object> Action5(Guid id) { return new { data = await service.Retry(id) }; }
  [HttpPost("demo/receiver")] public object Receiver([FromBody] JsonElement j) => new { data = new { received = true } };
}
