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
        Assert.Equal("変換するファイルを選択してください。", result.Message);
    }

    [Fact]
    public void Validate_ReturnsError_WhenExtensionIsUnsupported()
    {
        var service = CreateService();
        var file = CreateFile("notes.exe", 10);

        var result = service.Validate(file);

        Assert.False(result.IsValid);
        Assert.Equal("このファイル形式は受け付けていません。対応拡張子を確認してください。", result.Message);
    }

    [Fact]
    public void Validate_ReturnsError_WhenFileSizeExceedsLimit()
    {
        var service = CreateService(maxSizeBytes: 4);
        var file = CreateFile("notes.txt", 10);

        var result = service.Validate(file);

        Assert.False(result.IsValid);
        Assert.Contains("ファイルサイズが上限を超えています", result.Message);
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