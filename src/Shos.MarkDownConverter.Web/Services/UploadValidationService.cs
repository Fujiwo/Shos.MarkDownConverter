using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Options;

namespace Shos.MarkDownConverter.Web.Services;

public sealed class UploadValidationService
{
    private readonly MarkItDownOptions _options;

    public UploadValidationService(IOptions<MarkItDownOptions> options)
    {
        _options = options.Value;
    }

    public FileValidationResult Validate(IFormFile? file)
    {
        if (file is null)
        {
            return FileValidationResult.Invalid("変換するファイルを選択してください。");
        }

        if (file.Length <= 0)
        {
            return FileValidationResult.Invalid("空のファイルは変換できません。");
        }

        if (file.Length > _options.MaxUploadSizeBytes)
        {
            return FileValidationResult.Invalid($"ファイルサイズが上限を超えています。上限は {FormatFileSize(_options.MaxUploadSizeBytes)} です。");
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return FileValidationResult.Invalid("このファイル形式は受け付けていません。対応拡張子を確認してください。");
        }

        return FileValidationResult.Valid(extension);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        var kiloBytes = bytes / 1024d;
        if (kiloBytes < 1024)
        {
            return $"{kiloBytes:0.#} KB";
        }

        var megaBytes = kiloBytes / 1024d;
        return $"{megaBytes:0.#} MB";
    }
}

public sealed record FileValidationResult(bool IsValid, string? Message, string? Extension)
{
    public static FileValidationResult Valid(string extension) => new(true, null, extension);

    public static FileValidationResult Invalid(string message) => new(false, message, null);
}