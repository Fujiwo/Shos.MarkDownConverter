namespace Shos.MarkDownConverter.Web.Services;

public interface IMarkdownConversionService
{
    Task<MarkdownConversionResult> ConvertAsync(IFormFile file, CancellationToken cancellationToken);
}

public sealed record MarkdownConversionResult(string? Markdown, string? DownloadFileName, ConversionError? Error)
{
    public bool IsSuccess => Error is null;

    public static MarkdownConversionResult Success(string markdown, string downloadFileName) => new(markdown, downloadFileName, null);

    public static MarkdownConversionResult Failure(ConversionError error) => new(null, null, error);
}