namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// 外部プロセス起動を抽象化し、変換サービスから OS 依存の詳細を切り離します。
/// </summary>
public interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory, CancellationToken cancellationToken);
}

/// <summary>
/// 外部プロセスの終了コードと標準出力、標準エラーをまとめて返します。
/// </summary>
public sealed record ProcessExecutionResult(int ExitCode, string StandardOutput, string StandardError);