namespace Shos.MarkDownConverter.Web.Options;

public static class MarkItDownOptionsConfiguration
{
	public static long ResolveMaxUploadSizeBytes(IConfiguration configuration)
	{
		var configuredValue = configuration
			.GetSection(MarkItDownOptions.SectionName)
			.GetValue<long?>(nameof(MarkItDownOptions.MaxUploadSizeBytes));

		return configuredValue is > 0
			? configuredValue.Value
			: MarkItDownOptions.FallbackMaxUploadSizeBytes;
	}
}