using Microsoft.AspNetCore.Http.Features;
using Shos.MarkDownConverter.Web.Options;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Extensions;

public static class MarkDownConverterServiceCollectionExtensions
{
	public static IServiceCollection AddMarkDownConverter(
		this IServiceCollection services,
		IConfiguration configuration,
		string contentRootPath)
	{
		var converterOptionsSection = configuration.GetSection(MarkItDownOptions.SectionName);
		var maxUploadSize = MarkItDownOptionsConfiguration.ResolveMaxUploadSizeBytes(converterOptionsSection);
		var uploadLimitSettings = UploadLimitSettings.Create(maxUploadSize);

		services.Configure<FormOptions>(options =>
		{
			options.MultipartBodyLengthLimit = uploadLimitSettings.MultipartBodyLengthLimit;
		});

		services
			.AddOptions<MarkItDownOptions>()
			.Bind(converterOptionsSection)
			.PostConfigure(options => options.Normalize(contentRootPath, uploadLimitSettings.MaxUploadSizeBytes));

		services.AddSingleton(uploadLimitSettings);
		services.AddSingleton<UploadValidationService>();
		services.AddSingleton<MarkItDownErrorFormatter>();
		services.AddSingleton<IProcessRunner, ExternalProcessRunner>();
		services.AddScoped<IMarkdownConversionService, MarkItDownConversionService>();

		return services;
	}
}