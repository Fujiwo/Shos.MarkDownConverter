using Microsoft.AspNetCore.Http.Features;
using Shos.MarkDownConverter.Web.Options;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Extensions;

/// <summary>
/// MarkItDown 変換に必要な設定とサービスを DI コンテナーへ登録します。
/// </summary>
public static class MarkDownConverterServiceCollectionExtensions
{
	public static IServiceCollection AddMarkDownConverter(
		this IServiceCollection services,
		IConfiguration configuration,
		string contentRootPath)
	{
		var converterOptionsSection = configuration.GetSection(MarkItDownOptions.SectionName);
		// multipart のオーバーヘッドを見込んで、表示用の上限値と受信上限を分けて計算する。
		var maxUploadSize = MarkItDownOptionsConfiguration.ResolveMaxUploadSizeBytes(converterOptionsSection);
		var uploadLimitSettings = UploadLimitSettings.Create(maxUploadSize);

		services.Configure<FormOptions>(options =>
		{
			options.MultipartBodyLengthLimit = uploadLimitSettings.MultipartBodyLengthLimit;
		});

		services
			.AddOptions<MarkItDownOptions>()
			.Bind(converterOptionsSection)
			// bind 後に正規化して、相対パスや拡張子の表記ゆれを実行前に吸収する。
			.PostConfigure(options => options.Normalize(contentRootPath, uploadLimitSettings.MaxUploadSizeBytes));

		services.AddSingleton(uploadLimitSettings);
		services.AddSingleton<UploadValidationService>();
		services.AddSingleton<MarkItDownErrorFormatter>();
		services.AddSingleton<IProcessRunner, ExternalProcessRunner>();
		services.AddScoped<IMarkdownConversionService, MarkItDownConversionService>();

		return services;
	}
}