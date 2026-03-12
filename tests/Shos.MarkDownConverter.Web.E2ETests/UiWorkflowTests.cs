using Microsoft.Playwright;

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
        await page.WaitForFunctionAsync("() => document.getElementById('result-panel') && !document.getElementById('result-panel').hidden");

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

        var errorMessage = await page.TextContentAsync("#error-message");
        Assert.Contains("このファイル形式は受け付けていません", errorMessage);
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

        var errorMessage = await page.TextContentAsync("#error-message");
        Assert.Contains("Python 実行環境", errorMessage);
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