using Shos.MarkDownConverter.Web.Services;

namespace Shos.MarkDownConverter.Web.Tests;

public sealed class MarkdownDownloadFileNameBuilderTests
{
	[Fact]
	public void Build_ReturnsMarkdownFileName_WhenBaseNameIsValid()
	{
		var result = MarkdownDownloadFileNameBuilder.Build("report.docx");

		Assert.Equal("report.md", result);
	}

	[Fact]
	public void Build_SanitizesInvalidCharacters()
	{
		var result = MarkdownDownloadFileNameBuilder.Build("re*port?.docx");

		Assert.Equal("re-port-.md", result);
	}
}