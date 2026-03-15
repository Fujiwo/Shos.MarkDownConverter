namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// アップロードされたファイルを Markdown へ変換する処理の抽象化です。
/// </summary>
public interface IMarkdownConversionService
{
    Task<MarkdownConversionResult> ConvertAsync(IFormFile file, CancellationToken cancellationToken);
}

/// <summary>
/// 変換結果を、成功時と失敗時で同じ戻り値として扱うためのコンテナーです。
/// </summary>
public sealed record MarkdownConversionResult(string? Markdown, string? DownloadFileName, ConversionError? Error)
{
    public bool IsSuccess => Error is null;

    public static MarkdownConversionResult Success(string markdown, string downloadFileName) => new(markdown, downloadFileName, null);

    public static MarkdownConversionResult Failure(ConversionError error) => new(null, null, error);
}