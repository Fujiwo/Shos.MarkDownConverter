namespace Shos.MarkDownConverter.Web.Models;

/// <summary>
/// 画面へそのまま表示できるように整形したエラー情報を返します。
/// </summary>
public sealed record ErrorResponse(
	string Code,
	string Message,
	IReadOnlyList<string> PossibleCauses,
	IReadOnlyList<string> Actions);