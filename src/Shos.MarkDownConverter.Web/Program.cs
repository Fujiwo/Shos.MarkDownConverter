using Microsoft.AspNetCore.Diagnostics;
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
		var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
		if (IsPayloadTooLargeException(exception))
		{
			await Results.Json(
				CreateErrorResponse(CreateFileTooLargeError(maxUploadSize)),
				statusCode: StatusCodes.Status413PayloadTooLarge).ExecuteAsync(context);
			return;
		}

		await Results.Problem(
			statusCode: StatusCodes.Status500InternalServerError,
			title: "Unexpected error",
			detail: "予期しないエラーが発生しました。時間をおいて再試行してください。").ExecuteAsync(context);
	});
});

app.Use(async (context, next) =>
{
	if (HttpMethods.IsPost(context.Request.Method)
		&& context.Request.Path.Equals("/api/convert", StringComparison.OrdinalIgnoreCase)
		&& context.Request.ContentLength is long contentLength
		&& contentLength > maxUploadSize)
	{
		await Results.Json(
			CreateErrorResponse(CreateFileTooLargeError(maxUploadSize)),
			statusCode: StatusCodes.Status413PayloadTooLarge).ExecuteAsync(context);
		return;
	}

	await next(context);
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
		var error = validation.Error ?? throw new InvalidOperationException("Validation error was not provided.");
		return Results.Json(CreateErrorResponse(error), statusCode: error.StatusCode);
	}

	var result = await converter.ConvertAsync(file!, cancellationToken);
	if (result.IsSuccess)
	{
		return Results.Ok(new ConvertResponse(result.Markdown!, result.DownloadFileName!));
	}

	var conversionError = result.Error ?? throw new InvalidOperationException("Conversion error was not provided.");
	return Results.Json(CreateErrorResponse(conversionError), statusCode: conversionError.StatusCode);
})
.DisableAntiforgery();

app.MapFallbackToFile("index.html");

app.Run();

static ErrorResponse CreateErrorResponse(ConversionError error)
{
	return new ErrorResponse(
		error.Code,
		error.Message,
		error.PossibleCauses,
		error.Actions);
}

static ConversionError CreateFileTooLargeError(long maxUploadSize)
{
	return new ConversionError(
		StatusCodes.Status413PayloadTooLarge,
		"file-too-large",
		$"ファイルサイズが上限を超えています。現在の上限は {FormatFileSize(maxUploadSize)} です。",
		[
			"選択したファイルがこのアプリのアップロード上限を超えています。"
		],
		[
			"ファイルを小さくして再試行してください。",
			$"管理者は設定で上限値 {FormatFileSize(maxUploadSize)} を見直してください。"
		],
		null);
}

static string FormatFileSize(long bytes)
{
	if (bytes < 1024)
	{
		return $"{bytes} B";
	}

	var kiloBytes = bytes / 1024d;
	if (kiloBytes < 1024)
	{
		return $"{kiloBytes:0.#} KB";
	}

	var megaBytes = kiloBytes / 1024d;
	return $"{megaBytes:0.#} MB";
}

static bool IsPayloadTooLargeException(Exception? exception)
{
	for (var current = exception; current is not null; current = current.InnerException)
	{
		if (current is BadHttpRequestException badHttpRequestException
			&& badHttpRequestException.StatusCode == StatusCodes.Status413PayloadTooLarge)
		{
			return true;
		}

		if (current is InvalidDataException invalidDataException && ContainsPayloadTooLargeMessage(invalidDataException.Message))
		{
			return true;
		}

		if (current is BadHttpRequestException fallbackBadRequestException && ContainsPayloadTooLargeMessage(fallbackBadRequestException.Message))
		{
			return true;
		}
	}

	return false;
}

static bool ContainsPayloadTooLargeMessage(string message)
{
	return message.Contains("Multipart body length limit", StringComparison.OrdinalIgnoreCase)
		|| message.Contains("Request body too large", StringComparison.OrdinalIgnoreCase)
		|| message.Contains("body length limit", StringComparison.OrdinalIgnoreCase);
}

public partial class Program;
