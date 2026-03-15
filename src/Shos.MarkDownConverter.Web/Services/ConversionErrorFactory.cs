using Shos.MarkDownConverter.Web.Models;

namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// 変換関連の内部エラーを、UI がそのまま表示しやすい応答モデルへ変換します。
/// </summary>
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
		// 上限値は利用者向けメッセージと管理者向け案内の両方で使うため、ここで見やすい表記へそろえる。
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