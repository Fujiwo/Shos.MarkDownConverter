namespace Shos.MarkDownConverter.Web.Models;

/// <summary>
/// 変換成功時に、Markdown 本文とダウンロード用ファイル名を返します。
/// </summary>
public sealed record ConvertResponse(string Markdown, string DownloadFileName);