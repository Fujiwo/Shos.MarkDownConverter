using System.Diagnostics;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Tests;

public sealed class ExternalProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_ThrowsOperationCanceledException_WhenCancellationIsRequested()
    {
        var runner = new ExternalProcessRunner();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            runner.RunAsync(
                "powershell",
                ["-NoProfile", "-Command", "Start-Sleep -Seconds 30"],
                null,
                cancellationTokenSource.Token));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }
}