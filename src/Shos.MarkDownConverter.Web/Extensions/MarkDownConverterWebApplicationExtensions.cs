using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Models;
using Shos.MarkDownConverter.Web.Options;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Extensions;

public static class MarkDownConverterWebApplicationExtensions
{
	public static WebApplication UseMarkDownConverterExceptionHandling(this WebApplication app)
	{
		var uploadLimitSettings = app.Services.GetRequiredService<UploadLimitSettings>();

		app.UseExceptionHandler(errorApp =>
		{
			errorApp.Run(async context =>
			{
				var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
				if (PayloadTooLargeDetector.IsPayloadTooLarge(exception))
				{
					await Results.Json(
						ConversionErrorFactory.CreateResponse(ConversionErrorFactory.CreateFileTooLarge(uploadLimitSettings.MaxUploadSizeBytes)),
						statusCode: StatusCodes.Status413PayloadTooLarge).ExecuteAsync(context);
					return;
				}

				await Results.Problem(
					statusCode: StatusCodes.Status500InternalServerError,
					title: "Unexpected error",
					detail: "予期しないエラーが発生しました。時間をおいて再試行してください。").ExecuteAsync(context);
			});
		});

		return app;
	}

	public static WebApplication UseMarkDownConverterRequestLimits(this WebApplication app)
	{
		var uploadLimitSettings = app.Services.GetRequiredService<UploadLimitSettings>();

		app.Use(async (context, next) =>
		{
			if (HttpMethods.IsPost(context.Request.Method)
				&& context.Request.Path.Equals("/api/convert", StringComparison.OrdinalIgnoreCase)
				&& context.Request.ContentLength is long contentLength
				&& contentLength > uploadLimitSettings.MultipartBodyLengthLimit)
			{
				await Results.Json(
					ConversionErrorFactory.CreateResponse(ConversionErrorFactory.CreateFileTooLarge(uploadLimitSettings.MaxUploadSizeBytes)),
					statusCode: StatusCodes.Status413PayloadTooLarge).ExecuteAsync(context);
				return;
			}

			await next(context);
		});

		return app;
	}

	public static WebApplication MapMarkDownConverterEndpoints(this WebApplication app)
	{
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
				return Results.Json(ConversionErrorFactory.CreateResponse(error), statusCode: error.StatusCode);
			}

			var result = await converter.ConvertAsync(file!, cancellationToken);
			if (result.IsSuccess)
			{
				return Results.Ok(new ConvertResponse(result.Markdown!, result.DownloadFileName!));
			}

			var conversionError = result.Error ?? throw new InvalidOperationException("Conversion error was not provided.");
			return Results.Json(ConversionErrorFactory.CreateResponse(conversionError), statusCode: conversionError.StatusCode);
		})
		.DisableAntiforgery();

		return app;
	}
}