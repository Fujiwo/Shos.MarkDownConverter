using Microsoft.Extensions.Options;
using Shos.MarkDownConverter.Web.Options;

namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// 一時領域の準備から MarkItDown 実行、結果読込までの変換処理全体を統括します。
/// </summary>
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

    /// <summary>
    /// 一時領域の準備、MarkItDown 実行、結果確認を順に行い、成功時は Markdown、失敗時は構造化エラーを返します。
    /// </summary>
    public async Task<MarkdownConversionResult> ConvertAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        using var workspace = ConversionWorkspace.Create(_options.WorkingDirectoryRoot, extension, _logger);

        await SaveInputFileAsync(file, workspace.InputPath, cancellationToken);

        ProcessExecutionResult executionResult;
        try
        {
            executionResult = await RunConversionAsync(workspace, cancellationToken);
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or FileNotFoundException)
        {
			// プロセス自体を起動できない失敗は、変換失敗ではなく Python 環境の問題として扱う。
            var launchError = _errorFormatter.FormatLaunchFailure(exception);
            _logger.LogError(exception, "Failed to launch Python process. PythonPath: {PythonPath}", _options.PythonExecutablePath);
            return MarkdownConversionResult.Failure(launchError);
        }

        if (executionResult.ExitCode != 0)
        {
            // stderr は利用者向けに整形し、stdout と合わせて調査用ログにも残す。
            var error = _errorFormatter.FormatExecutionFailure(executionResult.ExitCode, executionResult.StandardError);
            _logger.LogWarning(
                "MarkItDown conversion failed. ExitCode: {ExitCode}, StdOut: {StdOut}, StdErr: {StdErr}",
                executionResult.ExitCode,
                executionResult.StandardOutput,
                executionResult.StandardError);
            return MarkdownConversionResult.Failure(error);
        }

        if (!File.Exists(workspace.OutputPath))
        {
			// 正常終了でも出力ファイルが無ければ成功扱いにせず、呼び出し側へ異常として返す。
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

        var markdown = await File.ReadAllTextAsync(workspace.OutputPath, cancellationToken);
        var downloadFileName = MarkdownDownloadFileNameBuilder.Build(file.FileName);
        return MarkdownConversionResult.Success(markdown, downloadFileName);
    }

    private async Task SaveInputFileAsync(IFormFile file, string inputPath, CancellationToken cancellationToken)
    {
        // 受信ストリームをそのまま一時ファイルへ書き出し、後続の Python プロセスから読める形にする。
        await using var targetStream = File.Create(inputPath);
        await file.CopyToAsync(targetStream, cancellationToken);
    }

    private Task<ProcessExecutionResult> RunConversionAsync(ConversionWorkspace workspace, CancellationToken cancellationToken)
    {
		// 引数は runner へ明示的に渡し、呼び出し構造をテストしやすい形に保つ。
        var arguments = new[]
        {
            "-m",
            _options.ModuleName,
            workspace.InputPath,
            "-o",
            workspace.OutputPath
        };

        return _processRunner.RunAsync(_options.PythonExecutablePath, arguments, workspace.WorkingDirectory, cancellationToken);
    }
}