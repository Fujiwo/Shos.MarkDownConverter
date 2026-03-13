using Microsoft.Playwright;
using Xunit.Sdk;

namespace Shos.MarkDownConverter.Web.E2ETests;

public sealed class UiWorkflowTests
{
    [Fact]
    public async Task ConvertTxt_ShowsMarkdownAndSupportsDownload()
    {
        await using var host = await TestAppHost.StartAsync();
        var samplePath = CreateTempFile("sample.txt", "# Sample\nThis is a UI test.");

        await using var browserSession = await BrowserSession.StartAsync();
        var page = await browserSession.Context.NewPageAsync();

        await PreparePageAsync(page, host.BaseUrl);
        await SelectFileAsync(page, samplePath, "sample.txt");
        await page.ClickAsync("#convert-button");
        await WaitForSuccessfulConversionAsync(page);

        var resultText = await page.InputValueAsync("#result-output");
        Assert.Contains("# Sample", resultText);
        Assert.Contains("This is a UI test.", resultText);

        var download = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.ClickAsync("#download-button");
        });

        Assert.Equal("sample.md", download.SuggestedFilename);
    }

    [Fact]
    public async Task ConvertTxt_AllowsCopyingMarkdownToClipboard()
    {
        await using var host = await TestAppHost.StartAsync();
        var samplePath = CreateTempFile("sample.txt", "clipboard content");

        await using var browserSession = await BrowserSession.StartAsync();
        var page = await browserSession.Context.NewPageAsync();

        await PreparePageAsync(page, host.BaseUrl);
        await SelectFileAsync(page, samplePath, "sample.txt");
        await page.ClickAsync("#convert-button");
        await WaitForSuccessfulConversionAsync(page);

        await page.ClickAsync("#copy-button");
        await page.WaitForFunctionAsync("() => document.getElementById('status-text').textContent === '変換結果をコピーしました。'");

        var clipboardText = await page.EvaluateAsync<string>("() => navigator.clipboard.readText()");
        Assert.Contains("clipboard content", clipboardText);
    }

    [Fact]
    public async Task CopyFailure_ShowsUserFacingError()
    {
        await using var host = await TestAppHost.StartAsync();
        var samplePath = CreateTempFile("sample.txt", "clipboard failure");

        await using var browserSession = await BrowserSession.StartAsync();
        var page = await browserSession.Context.NewPageAsync();

        await PreparePageAsync(page, host.BaseUrl);
        await SelectFileAsync(page, samplePath, "sample.txt");
        await page.ClickAsync("#convert-button");
        await WaitForSuccessfulConversionAsync(page);

        await page.EvaluateAsync(@"() => {
            Object.defineProperty(navigator, 'clipboard', {
                configurable: true,
                value: {
                    writeText: () => Promise.reject(new Error('denied'))
                }
            });
        }");

        await page.ClickAsync("#copy-button");
        await page.WaitForFunctionAsync("() => document.getElementById('error-panel') && !document.getElementById('error-panel').hidden");

        var errorMessage = await page.TextContentAsync("#error-message");
        var actionText = await page.TextContentAsync("#error-actions");
        Assert.Contains("クリップボードにコピーできませんでした", errorMessage);
        Assert.Contains("ダウンロード機能", actionText);
    }

    [Fact]
    public async Task UnsupportedExtension_ShowsErrorMessage()
    {
        await using var host = await TestAppHost.StartAsync();
        var samplePath = CreateTempFile("sample.exe", "not supported");

        await using var browserSession = await BrowserSession.StartAsync();
        var page = await browserSession.Context.NewPageAsync();

        await PreparePageAsync(page, host.BaseUrl);
        await SelectFileAsync(page, samplePath, "sample.exe");
        await page.ClickAsync("#convert-button");
        await page.WaitForFunctionAsync("() => document.getElementById('error-panel') && !document.getElementById('error-panel').hidden");
        await page.WaitForFunctionAsync("() => document.querySelectorAll('#error-causes li').length > 0 && document.querySelectorAll('#error-actions li').length > 0");

        var errorMessage = await page.TextContentAsync("#error-message");
        var causeText = await page.TextContentAsync("#error-causes");
        var actionText = await page.TextContentAsync("#error-actions");
        Assert.Contains("このファイル形式は現在の設定では受け付けていません", errorMessage);
        Assert.Contains("許可対象", causeText);
        Assert.Contains("対応拡張子一覧", actionText);
    }

    [Fact]
    public async Task MissingPython_ShowsDependencyError()
    {
        await using var host = await TestAppHost.StartAsync(new Dictionary<string, string>
        {
            ["MarkItDown__PythonExecutablePath"] = "missing-python.exe"
        });
        var samplePath = CreateTempFile("sample.txt", "python missing test");

        await using var browserSession = await BrowserSession.StartAsync();
        var page = await browserSession.Context.NewPageAsync();

        await PreparePageAsync(page, host.BaseUrl);
        await SelectFileAsync(page, samplePath, "sample.txt");
        await page.ClickAsync("#convert-button");
        await page.WaitForFunctionAsync("() => document.getElementById('error-panel') && !document.getElementById('error-panel').hidden");
        await page.WaitForFunctionAsync("() => document.querySelectorAll('#error-causes li').length > 0 && document.querySelectorAll('#error-actions li').length > 0");

        var errorMessage = await page.TextContentAsync("#error-message");
        var causeText = await page.TextContentAsync("#error-causes");
        var actionText = await page.TextContentAsync("#error-actions");
        Assert.Contains("Python 実行環境", errorMessage);
        Assert.Contains("Python 実行パス", causeText);
        Assert.Contains("PythonExecutablePath", actionText);
    }

    [Fact]
    public async Task FileTooLarge_ShowsStructuredErrorMessage()
    {
        await using var host = await TestAppHost.StartAsync(new Dictionary<string, string>
        {
            ["MarkItDown__MaxUploadSizeBytes"] = "128"
        });
        var samplePath = CreateTempFile("large.txt", new string('a', 2048));

        await using var browserSession = await BrowserSession.StartAsync();
        var page = await browserSession.Context.NewPageAsync();

        await PreparePageAsync(page, host.BaseUrl);
        await SelectFileAsync(page, samplePath, "large.txt");
        await page.ClickAsync("#convert-button");
        await page.WaitForFunctionAsync("() => document.getElementById('error-panel') && !document.getElementById('error-panel').hidden");

        var errorMessage = await page.TextContentAsync("#error-message");
        var causeText = await page.TextContentAsync("#error-causes");
        Assert.Contains("ファイルサイズが上限を超えています", errorMessage);
        Assert.Contains("アップロード上限", causeText);
    }

    private static async Task PreparePageAsync(IPage page, string baseUrl)
    {
        await page.GotoAsync(baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForFunctionAsync("() => document.getElementById('supported-extensions') && document.getElementById('supported-extensions').textContent !== '読み込み中...'");
    }

    private static async Task SelectFileAsync(IPage page, string path, string expectedName)
    {
        await page.Locator("#file-input").SetInputFilesAsync(path);
        await page.WaitForFunctionAsync(
            "name => document.getElementById('selected-file') && document.getElementById('selected-file').textContent === name",
            expectedName);
    }

    private static async Task WaitForSuccessfulConversionAsync(IPage page)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await page.Locator("#result-panel").IsVisibleAsync())
            {
                return;
            }

            if (await page.Locator("#error-panel").IsVisibleAsync())
            {
                var errorMessage = await page.TextContentAsync("#error-message") ?? "エラー内容を取得できませんでした。";
                throw new XunitException($"変換が成功せず、エラーが表示されました: {errorMessage}");
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("変換結果またはエラー表示の待機がタイムアウトしました。");
    }

    private sealed class BrowserSession : IAsyncDisposable
    {
        private BrowserSession(IPlaywright playwright, IBrowser browser, IBrowserContext context)
        {
            Playwright = playwright;
            Browser = browser;
            Context = context;
        }

        public IPlaywright Playwright { get; }

        public IBrowser Browser { get; }

        public IBrowserContext Context { get; }

        public static async Task<BrowserSession> StartAsync()
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true
            });
            await context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);

            return new BrowserSession(playwright, browser, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.CloseAsync();
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }

    private static string CreateTempFile(string fileName, string content)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"shos-markdownconverter-ui-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}