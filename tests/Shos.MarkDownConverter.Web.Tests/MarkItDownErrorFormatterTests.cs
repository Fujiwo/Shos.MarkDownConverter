using Microsoft.AspNetCore.Http;
using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Tests;

public sealed class MarkItDownErrorFormatterTests
{
    [Fact]
    public void FormatExecutionFailure_ReturnsDependencyError_WhenMarkItDownModuleIsMissing()
    {
        var formatter = new MarkItDownErrorFormatter();

        var error = formatter.FormatExecutionFailure(1, "ModuleNotFoundError: No module named 'markitdown'");

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, error.StatusCode);
        Assert.Equal("markitdown-missing", error.Code);
        Assert.Contains("MarkItDown が Python 環境に見つかりません", error.Message);
        Assert.Contains("インストール", string.Join(' ', error.Actions));
    }

    [Fact]
    public void FormatExecutionFailure_ReturnsUserFacingConversionError_ForGeneralFailure()
    {
        var formatter = new MarkItDownErrorFormatter();

        var error = formatter.FormatExecutionFailure(2, "Unsupported format");

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, error.StatusCode);
        Assert.Equal("conversion-failed", error.Code);
        Assert.Contains("変換できませんでした", error.Message);
        Assert.NotEmpty(error.PossibleCauses);
        Assert.NotEmpty(error.Actions);
        Assert.NotNull(error.DiagnosticDetails);
    }

    [Fact]
    public void FormatLaunchFailure_ReturnsDependencyError()
    {
        var formatter = new MarkItDownErrorFormatter();

        var error = formatter.FormatLaunchFailure(new FileNotFoundException("python not found"));

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, error.StatusCode);
        Assert.Equal("python-launch-failed", error.Code);
        Assert.Contains("Python 実行環境", error.Message);
        Assert.Contains("PythonExecutablePath", string.Join(' ', error.Actions));
    }
}