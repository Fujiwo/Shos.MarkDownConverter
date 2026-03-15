namespace Shos.MarkDownConverter.Web.Options;

/// <summary>
/// 表示用のファイル上限と multipart 受信上限をまとめて保持します。
/// </summary>
public sealed record UploadLimitSettings(long MaxUploadSizeBytes, long MultipartBodyLengthLimit)
{
	private const long MultipartOverheadBytes = 64 * 1024L;

	public static UploadLimitSettings Create(long maxUploadSizeBytes)
	{
		// multipart の境界文字列やヘッダー分を見込んで、受信上限だけ少し広く持たせる。
		return new UploadLimitSettings(maxUploadSizeBytes, maxUploadSizeBytes + MultipartOverheadBytes);
	}
}