using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using App.Repository;
namespace App.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610310002_Demo")]
public class Demo : Migration
{
  protected override void Up(MigrationBuilder m) { using var s = typeof(Demo).Assembly.GetManifestResourceStream("App.Migrations.demo.sql")!; using var r = new StreamReader(s); m.Sql(r.ReadToEnd()); }
  protected override void Down(MigrationBuilder m) => throw new NotSupportedException("Use a forward migration.");
}
