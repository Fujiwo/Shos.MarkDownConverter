using Shos.MarkDownConverter.Web.Models;

namespace Shos.MarkDownConverter.Web.Services;

public static class ConversionErrorFactory
{
	public static ErrorResponse CreateResponse(ConversionError error)
	{
		return new ErrorResponse(
			error.Code,
			error.Message,
			error.PossibleCauses,
			error.Actions);
	}

	public static ConversionError CreateFileTooLarge(long maxUploadSizeBytes)
	{
		var formattedMaxUploadSize = FileSizeFormatter.Format(maxUploadSizeBytes);

		return new ConversionError(
			StatusCodes.Status413PayloadTooLarge,
			"file-too-large",
			$"ファイルサイズが上限を超えています。現在の上限は {formattedMaxUploadSize} です。",
			[
				"選択したファイルがこのアプリのアップロード上限を超えています。"
			],
			[
				"ファイルを小さくして再試行してください。",
				$"管理者は設定で上限値 {formattedMaxUploadSize} を見直してください。"
			],
			null);
	}
}