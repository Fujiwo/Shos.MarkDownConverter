using Shos.MarkDownConverter.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Program.cs には起動順だけを残し、詳細な配線は拡張メソッド側へ寄せる。
builder.Services.AddMarkDownConverter(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

app.UseMarkDownConverterExceptionHandling();
app.UseMarkDownConverterRequestLimits();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapMarkDownConverterEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

/// <summary>
/// テスト プロジェクトから top-level Program を参照できるようにするための補助型です。
/// </summary>
public partial class Program;
