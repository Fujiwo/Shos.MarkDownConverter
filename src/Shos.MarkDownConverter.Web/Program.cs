using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Models;
using Shos.MarkDownConverter.Web.Options;
using Shos.MarkDownConverter.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var converterOptionsSection = builder.Configuration.GetSection(MarkItDownOptions.SectionName);
var maxUploadSize = converterOptionsSection.GetValue<long?>(nameof(MarkItDownOptions.MaxUploadSizeBytes))
	?? MarkItDownOptions.DefaultMaxUploadSizeBytes;

builder.Services.Configure<FormOptions>(options =>
{
	options.MultipartBodyLengthLimit = maxUploadSize;
});

builder.Services
	.AddOptions<MarkItDownOptions>()
	.Bind(converterOptionsSection)
	.PostConfigure(options => options.Normalize(builder.Environment.ContentRootPath));

builder.Services.AddSingleton<UploadValidationService>();
builder.Services.AddSingleton<MarkItDownErrorFormatter>();
builder.Services.AddSingleton<IProcessRunner, ExternalProcessRunner>();
builder.Services.AddScoped<IMarkdownConversionService, MarkItDownConversionService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		await Results.Problem(
			statusCode: StatusCodes.Status500InternalServerError,
			title: "Unexpected error",
			detail: "予期しないエラーが発生しました。時間をおいて再試行してください。").ExecuteAsync(context);
	});
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/options", (IOptions<MarkItDownOptions> options) =>
{
	var value = options.Value;
	return Results.Ok(new UiOptionsResponse(value.AllowedExtensions, value.MaxUploadSizeBytes));
});

app.MapPost("/api/convert", async (
	IFormFile? file,
	UploadValidationService validator,
	IMarkdownConversionService converter,
	CancellationToken cancellationToken) =>
{
	var validation = validator.Validate(file);
	if (!validation.IsValid)
	{
		return Results.Json(
			new ErrorResponse(
				validation.Error!.Code,
				validation.Error.Message,
				validation.Error.PossibleCauses,
				validation.Error.Actions),
			statusCode: validation.Error.StatusCode);
	}

	var result = await converter.ConvertAsync(file!, cancellationToken);
	if (result.IsSuccess)
	{
		return Results.Ok(new ConvertResponse(result.Markdown!, result.DownloadFileName!));
	}

	return Results.Json(
		new ErrorResponse(
			result.Error!.Code,
			result.Error.Message,
			result.Error.PossibleCauses,
			result.Error.Actions),
		statusCode: result.Error.StatusCode);
})
.DisableAntiforgery();

app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
