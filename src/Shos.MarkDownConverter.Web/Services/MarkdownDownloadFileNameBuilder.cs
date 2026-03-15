namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// 元ファイル名をもとに、ダウンロード用の安全な Markdown ファイル名を作成します。
/// </summary>
public static class MarkdownDownloadFileNameBuilder
{
	public static string Build(string originalFileName)
	{
		var baseName = Path.GetFileNameWithoutExtension(originalFileName);
		var invalidCharacters = Path.GetInvalidFileNameChars();
		// 保存先の環境で使えない文字は置き換え、元の名前の雰囲気だけを残す。
		var sanitized = new string(baseName.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray()).Trim();

		return string.IsNullOrWhiteSpace(sanitized) ? "converted.md" : $"{sanitized}.md";
	}
}