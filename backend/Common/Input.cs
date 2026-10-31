using System.Text.Json;
using System.Globalization;
namespace App.Common;

public class ApiException(int status, string message) : Exception(message) { public int Status => status; }
public static class Input
{
  public static string Text(JsonElement j, string k, int max, bool required = false)
  {
    if (j.ValueKind != JsonValueKind.Object) throw new ApiException(400, "JSON nesnesi gerekli.");
    if (!j.TryGetProperty(k, out var v)) { if (required) throw new ApiException(400, k + " gerekli."); return ""; }
    if (v.ValueKind != JsonValueKind.String) throw new ApiException(400, k + " metin olmalÄ±.");
    var s = v.GetString()!.Trim(); if (s.Length > max || (required && s.Length == 0)) throw new ApiException(400, k + " geÃ§ersiz."); return s;
  }
  public static int Number(JsonElement j, string k, int min, int max) { if (j.ValueKind != JsonValueKind.Object || !j.TryGetProperty(k, out var v) || v.ValueKind != JsonValueKind.Number || !v.TryGetInt32(out var n) || n < min || n > max) throw new ApiException(400, k + " geÃ§ersiz."); return n; }
  public static Guid Id(JsonElement j, string k) { if (!Guid.TryParse(Text(j, k, 36, true), out var id)) throw new ApiException(400, "GeÃ§ersiz kimlik."); return id; }
  public static DateOnly Date(JsonElement j, string k) { if (!DateOnly.TryParseExact(Text(j, k, 10, true), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) throw new ApiException(400, "GeÃ§ersiz tarih."); return d; }
  public static DateTimeOffset Instant(JsonElement j, string k) { var s = Text(j, k, 40, true); if (!(s.EndsWith('Z') || System.Text.RegularExpressions.Regex.IsMatch(s, @"[+-]\d{2}:\d{2}$")) || !DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) throw new ApiException(400, "Saat dilimi iÃ§eren tarih gerekli."); return d.ToUniversalTime(); }
  public static string Choice(JsonElement j, string k, params string[] values) { var s = Text(j, k, 30, true); if (!values.Contains(s)) throw new ApiException(400, k + " geÃ§ersiz."); return s; }
  public static string Email(JsonElement j) { var s = Text(j, "clientEmail", 200); if (s.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(s, @"^[^\s@]+@[^\s@]+\.[^\s@]+$")) throw new ApiException(400, "E-posta geÃ§ersiz."); return s; }
}
