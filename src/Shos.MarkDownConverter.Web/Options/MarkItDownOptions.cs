namespace Shos.MarkDownConverter.Web.Options;

public sealed class MarkItDownOptions
{
    public const string SectionName = "MarkItDown";
    public const long DefaultMaxUploadSizeBytes = 100 * 1024 * 1024;
    public static readonly string[] DefaultAllowedExtensions =
    [
        ".pdf",
        ".docx",
        ".pptx",
        ".xlsx",
        ".csv",
        ".json",
        ".xml",
        ".html",
        ".htm",
        ".txt",
        ".md",
        ".jpg",
        ".jpeg",
        ".png"
    ];

    public string PythonExecutablePath { get; set; } = "python";
    public string ModuleName { get; set; } = "markitdown";
    public long MaxUploadSizeBytes { get; set; } = DefaultMaxUploadSizeBytes;
    public string? WorkingDirectoryRoot { get; set; }
    public List<string> AllowedExtensions { get; set; } = [];

    public void Normalize(string? contentRootPath = null)
    {
        PythonExecutablePath = string.IsNullOrWhiteSpace(PythonExecutablePath) ? "python" : PythonExecutablePath.Trim();
        ModuleName = string.IsNullOrWhiteSpace(ModuleName) ? "markitdown" : ModuleName.Trim();
        MaxUploadSizeBytes = MaxUploadSizeBytes <= 0 ? DefaultMaxUploadSizeBytes : MaxUploadSizeBytes;

        if (!string.IsNullOrWhiteSpace(contentRootPath) && ShouldResolveRelativePath(PythonExecutablePath))
        {
            PythonExecutablePath = Path.GetFullPath(Path.Combine(contentRootPath, PythonExecutablePath));
        }

        if (!string.IsNullOrWhiteSpace(WorkingDirectoryRoot) && !Path.IsPathRooted(WorkingDirectoryRoot))
        {
            var root = string.IsNullOrWhiteSpace(contentRootPath) ? Environment.CurrentDirectory : contentRootPath;
            WorkingDirectoryRoot = Path.GetFullPath(Path.Combine(root, WorkingDirectoryRoot));
        }

        if (AllowedExtensions.Count == 0)
        {
            AllowedExtensions = [.. DefaultAllowedExtensions];
        }

        AllowedExtensions = AllowedExtensions
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.Trim().StartsWith('.') ? extension.Trim().ToLowerInvariant() : $".{extension.Trim().ToLowerInvariant()}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(extension => extension, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool ShouldResolveRelativePath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return false;
        }

        return path.Contains(Path.DirectorySeparatorChar)
            || path.Contains(Path.AltDirectorySeparatorChar)
            || path.StartsWith(".", StringComparison.Ordinal);
    }
}