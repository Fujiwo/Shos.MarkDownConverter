using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

namespace Shos.MarkDownConverter.Web.E2ETests;

internal sealed class TestAppHost : IAsyncDisposable
{
    private readonly Process _process;
    private readonly StringBuilder _output = new();
    private readonly string? _runDirectory;

    private TestAppHost(Process process, string baseUrl, string? runDirectory)
    {
        _process = process;
        BaseUrl = baseUrl;
        _runDirectory = runDirectory;

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
        var buildOutputDirectory = Path.Combine(projectDirectory, "bin", "Debug", "net10.0");
        var projectDllPath = Path.Combine(buildOutputDirectory, "Shos.MarkDownConverter.Web.dll");

        if (!File.Exists(projectDllPath))
        {
            throw new FileNotFoundException("Web application output was not found. Build the solution before running E2E tests.", projectDllPath);
        }

        var runDirectory = CreateRunDirectory(buildOutputDirectory);
        var runnableDllPath = Path.Combine(runDirectory, "Shos.MarkDownConverter.Web.dll");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(runnableDllPath);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(baseUrl);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["MarkItDown__PythonExecutablePath"] = Path.Combine(repoRoot, ".venv", "Scripts", "python.exe");

        if (environmentOverrides is not null)
        {
            foreach (var pair in environmentOverrides)
            {
                startInfo.Environment[pair.Key] = pair.Value;
            }
        }

        var process = new Process { StartInfo = startInfo };
        var host = new TestAppHost(process, baseUrl, runDirectory);
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

            if (!string.IsNullOrWhiteSpace(_runDirectory) && Directory.Exists(_runDirectory))
            {
                try
                {
                    Directory.Delete(_runDirectory, recursive: true);
                }
                catch
                {
                }
            }
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

    private static string CreateRunDirectory(string buildOutputDirectory)
    {
        var runDirectory = Path.Combine(Path.GetTempPath(), $"shos-markdownconverter-e2e-{Guid.NewGuid():N}");
        Directory.CreateDirectory(runDirectory);

        foreach (var filePath in Directory.GetFiles(buildOutputDirectory))
        {
            var destinationPath = Path.Combine(runDirectory, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, overwrite: true);
        }

        return runDirectory;
    }
}