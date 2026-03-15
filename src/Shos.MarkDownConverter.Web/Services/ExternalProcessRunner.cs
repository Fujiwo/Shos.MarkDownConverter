using System.Diagnostics;
using System.Text;

namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// 外部プロセスを起動し、標準出力と標準エラーを回収して呼び出し元へ返します。
/// </summary>
public sealed class ExternalProcessRunner : IProcessRunner
{
    public async Task<ProcessExecutionResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? Environment.CurrentDirectory : workingDirectory
        };

        // 文字列連結で引用符を組み立てず、ArgumentList で OS に安全な引数分割を任せる。
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        var standardOutput = new StringBuilder();
        var standardError = new StringBuilder();

        process.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                standardOutput.AppendLine(eventArgs.Data);
            }
        };

        process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                standardError.AppendLine(eventArgs.Data);
            }
        };

        process.Start();
		// 出力は非同期で読み始め、標準出力と標準エラーの詰まりを避ける。
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
				// 変換中断後に子プロセスが残らないよう、プロセスツリーごと停止する。
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            throw;
        }

        return new ProcessExecutionResult(process.ExitCode, standardOutput.ToString(), standardError.ToString());
    }
}