using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
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
            "変換できませんでした。",
            ["対応形式を確認してください。"],
            null);

        await using var factory = new TestApplicationFactory(MarkdownConversionResult.Failure(error));
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", "dummy");

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();

        Assert.Equal((HttpStatusCode)StatusCodes.Status422UnprocessableEntity, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("変換できませんでした。", payload.Message);
    }

    [Fact]
    public async Task PostConvert_ReturnsServiceUnavailable_WhenPythonIsMissing()
    {
        var error = new ConversionError(
            StatusCodes.Status503ServiceUnavailable,
            "Python 実行環境が見つかりません。",
            ["Python を確認してください。"],
            null);

        await using var factory = new TestApplicationFactory(MarkdownConversionResult.Failure(error));
        using var client = factory.CreateClient();
        using var content = CreateMultipartContent("sample.docx", "dummy");

        using var response = await client.PostAsync("/api/convert", content);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Python 実行環境が見つかりません。", payload.Message);
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

        public TestApplicationFactory(MarkdownConversionResult result)
        {
            _result = result;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMarkdownConversionService>();
                services.AddSingleton<IMarkdownConversionService>(new StubMarkdownConversionService(_result));
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

    private sealed record ConvertResponseDto(string Markdown, string DownloadFileName);

    private sealed record ErrorResponseDto(string Message, IReadOnlyList<string> Tips);
}