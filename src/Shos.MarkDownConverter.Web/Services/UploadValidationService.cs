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
            return FileValidationResult.Invalid(new ConversionError(
                StatusCodes.Status400BadRequest,
                "file-required",
                "変換するファイルが選択されていません。",
                [
                    "まだファイルを選択していない可能性があります。",
                    "ブラウザーでファイル選択が完了していない可能性があります。"
                ],
                [
                    "変換したいファイルを 1 つ選択してから再度実行してください。",
                    "ファイル名が画面に表示されていることを確認してください。"
                ],
                null));
        }

        if (file.Length <= 0)
        {
            return FileValidationResult.Invalid(new ConversionError(
                StatusCodes.Status400BadRequest,
                "file-empty",
                "空のファイルは変換できません。",
                [
                    "選択したファイルの内容が空です。",
                    "ファイルの保存に失敗して中身が失われている可能性があります。"
                ],
                [
                    "内容が入っている元ファイルを選び直してください。",
                    "ファイルを開いて内容が存在するか確認してください。"
                ],
                null));
        }

        if (file.Length > _options.MaxUploadSizeBytes)
        {
            return FileValidationResult.Invalid(ConversionErrorFactory.CreateFileTooLarge(_options.MaxUploadSizeBytes) with
            {
                StatusCode = StatusCodes.Status400BadRequest
            });
        }

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return FileValidationResult.Invalid(new ConversionError(
                StatusCodes.Status400BadRequest,
                "unsupported-extension",
                "このファイル形式は現在の設定では受け付けていません。",
                [
                    "ファイル拡張子が許可対象に含まれていません。",
                    "MarkItDown が対応していても、このアプリの許可設定に入っていない可能性があります。"
                ],
                [
                    "画面に表示されている対応拡張子一覧を確認してください。",
                    "必要であれば管理者が AllowedExtensions の設定を見直してください。"
                ],
                $"fileName={file.FileName}; extension={extension ?? "(none)"}"));
        }

        return FileValidationResult.Valid(extension);
    }
}

public sealed record FileValidationResult(bool IsValid, ConversionError? Error, string? Extension)
{
    public static FileValidationResult Valid(string extension) => new(true, null, extension);

    public static FileValidationResult Invalid(ConversionError error) => new(false, error, null);
}