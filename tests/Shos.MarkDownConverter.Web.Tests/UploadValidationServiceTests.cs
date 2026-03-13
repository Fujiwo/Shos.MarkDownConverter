using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Options;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Tests;

public sealed class UploadValidationServiceTests
{
    [Fact]
    public void Validate_ReturnsError_WhenFileIsMissing()
    {
        var service = CreateService();

        var result = service.Validate(null);

        Assert.False(result.IsValid);
        Assert.Equal("file-required", result.Error!.Code);
        Assert.Equal("変換するファイルが選択されていません。", result.Error.Message);
        Assert.Contains("ファイル名が画面に表示", string.Join(' ', result.Error.Actions));
    }

    [Fact]
    public void Validate_ReturnsError_WhenExtensionIsUnsupported()
    {
        var service = CreateService();
        var file = CreateFile("notes.exe", 10);

        var result = service.Validate(file);

        Assert.False(result.IsValid);
        Assert.Equal("unsupported-extension", result.Error!.Code);
        Assert.Equal("このファイル形式は現在の設定では受け付けていません。", result.Error.Message);
        Assert.Contains("対応拡張子一覧", string.Join(' ', result.Error.Actions));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFileSizeExceedsLimit()
    {
        var service = CreateService(maxSizeBytes: 4);
        var file = CreateFile("notes.txt", 10);

        var result = service.Validate(file);

        Assert.False(result.IsValid);
        Assert.Equal("file-too-large", result.Error!.Code);
        Assert.Contains("ファイルサイズが上限を超えています", result.Error.Message);
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenFileIsSupported()
    {
        var service = CreateService();
        var file = CreateFile("report.docx", 10);

        var result = service.Validate(file);

        Assert.True(result.IsValid);
        Assert.Equal(".docx", result.Extension);
    }

    private static UploadValidationService CreateService(long maxSizeBytes = 1024)
    {
        return new UploadValidationService(Microsoft.Extensions.Options.Options.Create(new MarkItDownOptions
        {
            MaxUploadSizeBytes = maxSizeBytes,
            AllowedExtensions = [".txt", ".docx", ".pdf"]
        }));
    }

    private static IFormFile CreateFile(string fileName, int length)
    {
        var buffer = Enumerable.Repeat((byte)65, length).ToArray();
        var stream = new MemoryStream(buffer);
        return new FormFile(stream, 0, buffer.Length, "file", fileName);
    }
}