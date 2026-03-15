namespace Shos.MarkDownConverter.Web.Options;

/// <summary>
/// MarkItDown 設定から、アップロード上限の既定値解決だけを切り出して扱います。
/// </summary>
public static class MarkItDownOptionsConfiguration
{
	public static long ResolveMaxUploadSizeBytes(IConfiguration configuration)
	{
		// 無効値が来た場合でも、受信制限が未設定にならないように既定値へ戻す。
		var configuredValue = configuration
			.GetSection(MarkItDownOptions.SectionName)
			.GetValue<long?>(nameof(MarkItDownOptions.MaxUploadSizeBytes));

		return configuredValue is > 0
			? configuredValue.Value
			: MarkItDownOptions.FallbackMaxUploadSizeBytes;
	}
}