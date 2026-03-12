namespace Shos.MarkDownConverter.Web.Services;

public interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory, CancellationToken cancellationToken);
}

public sealed record ProcessExecutionResult(int ExitCode, string StandardOutput, string StandardError);