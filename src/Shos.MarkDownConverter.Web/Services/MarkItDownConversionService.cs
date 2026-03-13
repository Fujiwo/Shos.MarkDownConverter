using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Options;

namespace Shos.MarkDownConverter.Web.Services;

public sealed class MarkItDownConversionService : IMarkdownConversionService
{
    private readonly IProcessRunner _processRunner;
    private readonly MarkItDownOptions _options;
    private readonly MarkItDownErrorFormatter _errorFormatter;
    private readonly ILogger<MarkItDownConversionService> _logger;

    public MarkItDownConversionService(
        IProcessRunner processRunner,
        IOptions<MarkItDownOptions> options,
        MarkItDownErrorFormatter errorFormatter,
        ILogger<MarkItDownConversionService> logger)
    {
        _processRunner = processRunner;
        _options = options.Value;
        _errorFormatter = errorFormatter;
        _logger = logger;
    }

    public async Task<MarkdownConversionResult> ConvertAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var workingDirectoryRoot = string.IsNullOrWhiteSpace(_options.WorkingDirectoryRoot)
            ? Path.GetTempPath()
            : _options.WorkingDirectoryRoot;
        var workingDirectory = Path.Combine(workingDirectoryRoot, $"shos-markdownconverter-{Guid.NewGuid():N}");

        Directory.CreateDirectory(workingDirectory);

        var inputPath = Path.Combine(workingDirectory, $"input{extension}");
        var outputPath = Path.Combine(workingDirectory, "output.md");

        try
        {
            await using (var targetStream = File.Create(inputPath))
            {
                await file.CopyToAsync(targetStream, cancellationToken);
            }

            var arguments = new[]
            {
                "-m",
                _options.ModuleName,
                inputPath,
                "-o",
                outputPath
            };

            ProcessExecutionResult executionResult;
            try
            {
                executionResult = await _processRunner.RunAsync(_options.PythonExecutablePath, arguments, workingDirectory, cancellationToken);
            }
            catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or FileNotFoundException)
            {
                var launchError = _errorFormatter.FormatLaunchFailure(exception);
                _logger.LogError(exception, "Failed to launch Python process. PythonPath: {PythonPath}", _options.PythonExecutablePath);
                return MarkdownConversionResult.Failure(launchError);
            }

            if (executionResult.ExitCode != 0)
            {
                var error = _errorFormatter.FormatExecutionFailure(executionResult.ExitCode, executionResult.StandardError);
                _logger.LogWarning(
                    "MarkItDown conversion failed. ExitCode: {ExitCode}, StdOut: {StdOut}, StdErr: {StdErr}",
                    executionResult.ExitCode,
                    executionResult.StandardOutput,
                    executionResult.StandardError);
                return MarkdownConversionResult.Failure(error);
            }

            if (!File.Exists(outputPath))
            {
                var error = new ConversionError(
                    StatusCodes.Status502BadGateway,
                    "output-missing",
                    "変換結果を取得できませんでした。",
                    [
                        "MarkItDown の実行は完了しましたが、出力ファイルが作成されませんでした。",
                        "一時ディレクトリへの書き込みに失敗した可能性があります。"
                    ],
                    [
                        "しばらく待ってから再試行してください。",
                        "問題が続く場合はアプリケーションログを確認してください。"
                    ],
                    "MarkItDown completed without output file.");
                _logger.LogWarning("MarkItDown completed successfully but no output file was produced.");
                return MarkdownConversionResult.Failure(error);
            }

            var markdown = await File.ReadAllTextAsync(outputPath, cancellationToken);
            var downloadFileName = BuildDownloadFileName(file.FileName);
            return MarkdownConversionResult.Success(markdown, downloadFileName);
        }
        finally
        {
            try
            {
                if (Directory.Exists(workingDirectory))
                {
                    Directory.Delete(workingDirectory, recursive: true);
                }
            }
            catch (Exception cleanupException)
            {
                _logger.LogWarning(cleanupException, "Failed to clean temporary directory {WorkingDirectory}", workingDirectory);
            }
        }
    }

    private static string BuildDownloadFileName(string originalFileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(baseName.Select(character => invalidCharacters.Contains(character) ? '-' : character).ToArray()).Trim();

        return string.IsNullOrWhiteSpace(sanitized) ? "converted.md" : $"{sanitized}.md";
    }
}