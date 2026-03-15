namespace Shos.MarkDownConverter.Web.Models;

/// <summary>
/// 画面初期化時に必要な対応拡張子一覧とアップロード上限を返します。
/// </summary>
public sealed record UiOptionsResponse(IReadOnlyList<string> AllowedExtensions, long MaxUploadSizeBytes);