using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Options;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Tests;

public sealed class MarkItDownConversionServiceTests
{
    [Fact]
    public void Normalize_LeavesBarePythonCommandUntouched()
    {
        var options = new MarkItDownOptions
        {
            PythonExecutablePath = "python"
        };

        options.Normalize(Path.Combine("C:\\repo", "src", "Shos.MarkDownConverter.Web"));

        Assert.Equal("python", options.PythonExecutablePath);
    }

    [Fact]
    public void Normalize_ResolvesRelativePythonPathAgainstContentRoot()
    {
        var contentRoot = Path.Combine("C:\\repo", "src", "Shos.MarkDownConverter.Web");
        var options = new MarkItDownOptions
        {
            PythonExecutablePath = Path.Combine("..", "..", ".venv", "Scripts", "python.exe")
        };

        options.Normalize(contentRoot);

        Assert.Equal(Path.GetFullPath(Path.Combine(contentRoot, "..", "..", ".venv", "Scripts", "python.exe")), options.PythonExecutablePath);
    }

    [Fact]
    public void Normalize_KeepsAbsolutePythonPathUntouched()
    {
        var absolutePath = Path.GetFullPath(Path.Combine("C:\\tools", "python.exe"));
        var options = new MarkItDownOptions
        {
            PythonExecutablePath = absolutePath
        };

        options.Normalize(Path.Combine("C:\\repo", "src", "Shos.MarkDownConverter.Web"));

        Assert.Equal(absolutePath, options.PythonExecutablePath);
    }

    [Fact]
    public async Task ConvertAsync_ReturnsMarkdown_WhenProcessSucceeds()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"markdownconverter-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var runner = new FakeProcessRunner((_, arguments, _, _) =>
            {
                var outputPath = arguments[4];
                File.WriteAllText(outputPath, "# converted");
                return Task.FromResult(new ProcessExecutionResult(0, "ok", string.Empty));
            });

            var service = CreateService(runner, tempRoot);
            var file = CreateFile("report.docx", "source");

            var result = await service.ConvertAsync(file, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("# converted", result.Markdown);
            Assert.Equal("report.md", result.DownloadFileName);
            Assert.Empty(Directory.GetDirectories(tempRoot));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ConvertAsync_ReturnsFormattedError_WhenProcessFails()
    {
        var service = CreateService(new FakeProcessRunner((_, _, _, _) =>
            Task.FromResult(new ProcessExecutionResult(2, string.Empty, "Unsupported format"))));
        var file = CreateFile("report.docx", "source");

        var result = await service.ConvertAsync(file, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.Error!.StatusCode);
    }

    [Fact]
    public async Task ConvertAsync_ReturnsDependencyError_WhenPythonCannotBeStarted()
    {
        var service = CreateService(new FakeProcessRunner((_, _, _, _) => throw new FileNotFoundException("python not found")));
        var file = CreateFile("report.docx", "source");

        var result = await service.ConvertAsync(file, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.Error!.StatusCode);
    }

    private static MarkItDownConversionService CreateService(IProcessRunner runner, string? tempRoot = null)
    {
        return new MarkItDownConversionService(
            runner,
            Microsoft.Extensions.Options.Options.Create(new MarkItDownOptions
            {
                PythonExecutablePath = "python",
                ModuleName = "markitdown",
                WorkingDirectoryRoot = tempRoot,
                AllowedExtensions = [".docx"]
            }),
            new MarkItDownErrorFormatter(),
            NullLogger<MarkItDownConversionService>.Instance);
    }

    private static IFormFile CreateFile(string fileName, string contents)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(contents);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly Func<string, IReadOnlyList<string>, string?, CancellationToken, Task<ProcessExecutionResult>> _handler;

        public FakeProcessRunner(Func<string, IReadOnlyList<string>, string?, CancellationToken, Task<ProcessExecutionResult>> handler)
        {
            _handler = handler;
        }

        public Task<ProcessExecutionResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory, CancellationToken cancellationToken)
        {
            return _handler(fileName, arguments, workingDirectory, cancellationToken);
        }
    }
}