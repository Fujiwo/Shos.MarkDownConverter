using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace Shos.MarkDownConverter.Web.E2ETests;

internal sealed class TestAppHost : IAsyncDisposable
{
    private readonly Process _process;
    private readonly StringBuilder _output = new();

    private TestAppHost(Process process, string baseUrl)
    {
        _process = process;
        BaseUrl = baseUrl;

        _process.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                _output.AppendLine(eventArgs.Data);
            }
        };

        _process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is not null)
            {
                _output.AppendLine(eventArgs.Data);
            }
        };
    }

    public string BaseUrl { get; }

    public static async Task<TestAppHost> StartAsync(IReadOnlyDictionary<string, string>? environmentOverrides = null)
    {
        var repoRoot = FindRepoRoot();
        var projectDirectory = Path.Combine(repoRoot, "src", "Shos.MarkDownConverter.Web");
        var port = GetFreeTcpPort();
        var baseUrl = $"http://127.0.0.1:{port}";
        var projectDllPath = Path.Combine(projectDirectory, "bin", "Debug", "net10.0", "Shos.MarkDownConverter.Web.dll");

        if (!File.Exists(projectDllPath))
        {
            throw new FileNotFoundException("Web application output was not found. Build the solution before running E2E tests.", projectDllPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(projectDllPath);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(baseUrl);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        if (environmentOverrides is not null)
        {
            foreach (var pair in environmentOverrides)
            {
                startInfo.Environment[pair.Key] = pair.Value;
            }
        }

        var process = new Process { StartInfo = startInfo };
        var host = new TestAppHost(process, baseUrl);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await host.WaitUntilReadyAsync();
        return host;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        finally
        {
            _process.Dispose();
        }
    }

    public string GetProcessOutput() => _output.ToString();

    private async Task WaitUntilReadyAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (_process.HasExited)
            {
                throw new InvalidOperationException($"Web application exited prematurely. Output:{Environment.NewLine}{GetProcessOutput()}");
            }

            try
            {
                using var response = await client.GetAsync($"{BaseUrl}/api/options");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Timed out waiting for the web application to start. Output:{Environment.NewLine}{GetProcessOutput()}");
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Shos.MarkDownConverter.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root could not be located from the test output directory.");
    }
}