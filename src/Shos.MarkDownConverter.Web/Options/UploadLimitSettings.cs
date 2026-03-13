namespace Shos.MarkDownConverter.Web.Options;

public sealed record UploadLimitSettings(long MaxUploadSizeBytes, long MultipartBodyLengthLimit)
{
	private const long MultipartOverheadBytes = 64 * 1024L;

	public static UploadLimitSettings Create(long maxUploadSizeBytes)
	{
		return new UploadLimitSettings(maxUploadSizeBytes, maxUploadSizeBytes + MultipartOverheadBytes);
	}
}