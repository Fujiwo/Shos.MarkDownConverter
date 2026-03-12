namespace Shos.MarkDownConverter.Web.Options;

public sealed class MarkItDownOptions
{
    public const string SectionName = "MarkItDown";
    public const long DefaultMaxUploadSizeBytes = 10 * 1024 * 1024;

    public string PythonExecutablePath { get; set; } = "python";
    public string ModuleName { get; set; } = "markitdown";
    public long MaxUploadSizeBytes { get; set; } = DefaultMaxUploadSizeBytes;
    public string? WorkingDirectoryRoot { get; set; }
    public List<string> AllowedExtensions { get; set; } =
    [
        ".pdf",
        ".docx",
        ".pptx",
        ".xlsx",
        ".xls",
        ".csv",
        ".json",
        ".xml",
        ".html",
        ".htm",
        ".txt",
        ".md",
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".tif",
        ".tiff",
        ".webp",
        ".wav",
        ".mp3",
        ".zip",
        ".epub",
        ".msg",
        ".eml"
    ];

    public void Normalize()
    {
        PythonExecutablePath = string.IsNullOrWhiteSpace(PythonExecutablePath) ? "python" : PythonExecutablePath.Trim();
        ModuleName = string.IsNullOrWhiteSpace(ModuleName) ? "markitdown" : ModuleName.Trim();
        MaxUploadSizeBytes = MaxUploadSizeBytes <= 0 ? DefaultMaxUploadSizeBytes : MaxUploadSizeBytes;

        AllowedExtensions = AllowedExtensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.Trim().StartsWith('.') ? extension.Trim().ToLowerInvariant() : $".{extension.Trim().ToLowerInvariant()}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(extension => extension, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}