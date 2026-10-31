using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using App.Repository;
namespace App.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610310001_Initial")]
public class Initial : Migration
{
  protected override void Up(MigrationBuilder m) { using var s = typeof(Initial).Assembly.GetManifestResourceStream("App.Migrations.initial.sql")!; using var r = new StreamReader(s); m.Sql(r.ReadToEnd()); }
  protected override void Down(MigrationBuilder m) => throw new NotSupportedException("Create a new forward migration to preserve records.");
}
