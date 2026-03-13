using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.IntegrationTests;

public sealed class ConvertEndpointTests
{
    [Fact]
    public async Task PostConvert_ReturnsMarkdown_WhenConversionSucceeds()
    {
        await using var factory = new TestApplicationFactory(MarkdownConversionResult.Success("# hello", "sample.md"));
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", "dummy");

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ConvertResponseDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("# hello", payload.Markdown);
        Assert.Equal("sample.md", payload.DownloadFileName);
    }

    [Fact]
    public async Task PostConvert_ReturnsError_WhenConversionFails()
    {
        var error = new ConversionError(
            StatusCodes.Status422UnprocessableEntity,
            "conversion-failed",
            "変換できませんでした。",
            ["入力ファイルを解釈できません。"],
            ["対応形式を確認してください。"],
            null);

        await using var factory = new TestApplicationFactory(MarkdownConversionResult.Failure(error));
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", "dummy");

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();

        Assert.Equal((HttpStatusCode)StatusCodes.Status422UnprocessableEntity, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("conversion-failed", payload.Code);
        Assert.Equal("変換できませんでした。", payload.Message);
        Assert.Contains("対応形式を確認してください。", payload.Actions);
    }

    [Fact]
    public async Task PostConvert_ReturnsServiceUnavailable_WhenPythonLaunchFails()
    {
        var error = new ConversionError(
            StatusCodes.Status503ServiceUnavailable,
            "python-launch-failed",
            "Python 実行環境を起動できませんでした。",
            ["設定した Python 実行パスにアクセスできません。"],
            ["PythonExecutablePath の設定値を確認してください。"],
            null);

        await using var factory = new TestApplicationFactory(MarkdownConversionResult.Failure(error));
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", "dummy");

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("python-launch-failed", payload.Code);
        Assert.Equal("Python 実行環境を起動できませんでした。", payload.Message);
        Assert.Contains("PythonExecutablePath の設定値を確認してください。", payload.Actions);
    }

    [Fact]
    public async Task PostConvert_ReturnsProblemDetails_WhenUnhandledExceptionOccurs()
    {
        await using var factory = new ExceptionApplicationFactory();
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", "dummy");

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Unexpected error", payload.Title);
        Assert.Contains("予期しないエラー", payload.Detail);
    }

    [Fact]
    public async Task PostConvert_ReturnsStructuredError_WhenPayloadIsTooLarge()
    {
        await using var factory = new TestApplicationFactory(
            MarkdownConversionResult.Success("# hello", "sample.md"),
            new Dictionary<string, string?>
            {
                ["MarkItDown:MaxUploadSizeBytes"] = "128"
            });
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", new string('a', 2048));

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.RequestEntityTooLarge,
            $"Expected 400 or 413 but got {(int)response.StatusCode} ({response.StatusCode}).");
        Assert.NotNull(payload);
        Assert.Equal("file-too-large", payload.Code);
        Assert.Contains("ファイルサイズが上限を超えています", payload.Message);
        Assert.Contains(payload.PossibleCauses, cause => cause.Contains("アップロード上限", StringComparison.Ordinal));
    }

    private static MultipartFormDataContent CreateMultipartContent(string fileName, string contents)
    {
        var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(contents))), "file", fileName);
        return multipartContent;
    }

    private sealed class TestApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly MarkdownConversionResult _result;
        private readonly IReadOnlyDictionary<string, string?>? _configurationOverrides;

        public TestApplicationFactory(MarkdownConversionResult result, IReadOnlyDictionary<string, string?>? configurationOverrides = null)
        {
            _result = result;
            _configurationOverrides = configurationOverrides;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            if (_configurationOverrides is not null)
            {
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(_configurationOverrides);
                });
            }

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMarkdownConversionService>();
                services.AddSingleton<IMarkdownConversionService>(new StubMarkdownConversionService(_result));
            });
        }
    }

    private sealed class ExceptionApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMarkdownConversionService>();
                services.AddSingleton<IMarkdownConversionService>(new ThrowingMarkdownConversionService());
            });
        }
    }

    private sealed class StubMarkdownConversionService : IMarkdownConversionService
    {
        private readonly MarkdownConversionResult _result;

        public StubMarkdownConversionService(MarkdownConversionResult result)
        {
            _result = result;
        }

        public Task<MarkdownConversionResult> ConvertAsync(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class ThrowingMarkdownConversionService : IMarkdownConversionService
    {
        public Task<MarkdownConversionResult> ConvertAsync(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("unexpected failure");
        }
    }

    private sealed record ConvertResponseDto(string Markdown, string DownloadFileName);

    private sealed record ErrorResponseDto(string Code, string Message, IReadOnlyList<string> PossibleCauses, IReadOnlyList<string> Actions);

    private sealed record ProblemDetailsDto(string Title, string Detail);
}