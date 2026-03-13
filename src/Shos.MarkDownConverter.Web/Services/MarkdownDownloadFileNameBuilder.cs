namespace Shos.MarkDownConverter.Web.Services;

public static class MarkdownDownloadFileNameBuilder
{
	public static string Build(string originalFileName)
	{
		var baseName = Path.GetFileNameWithoutExtension(originalFileName);
		var invalidCharacters = Path.GetInvalidFileNameChars();
		var sanitized = new string(baseName.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray()).Trim();

		return string.IsNullOrWhiteSpace(sanitized) ? "converted.md" : $"{sanitized}.md";
	}
}