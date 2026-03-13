using Microsoft.Extensions.Configuration;
using Shos.MarkDownConverter.Web.Options;

namespace Shos.MarkDownConverter.Web.Tests;

public sealed class MarkItDownOptionsTests
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
	public void ResolveMaxUploadSizeBytes_ReturnsConfiguredValue_WhenConfigurationContainsPositiveValue()
	{
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{MarkItDownOptions.SectionName}:{nameof(MarkItDownOptions.MaxUploadSizeBytes)}"] = "2048"
			})
			.Build();

		var result = MarkItDownOptionsConfiguration.ResolveMaxUploadSizeBytes(configuration);

		Assert.Equal(2048, result);
	}

	[Fact]
	public void ResolveMaxUploadSizeBytes_ReturnsFallback_WhenConfigurationIsMissing()
	{
		var configuration = new ConfigurationBuilder().Build();

		var result = MarkItDownOptionsConfiguration.ResolveMaxUploadSizeBytes(configuration);

		Assert.Equal(MarkItDownOptions.FallbackMaxUploadSizeBytes, result);
	}
}