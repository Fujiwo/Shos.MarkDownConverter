using Shos.MarkDownConverter.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarkDownConverter(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

app.UseMarkDownConverterExceptionHandling();
app.UseMarkDownConverterRequestLimits();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapMarkDownConverterEndpoints();

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
