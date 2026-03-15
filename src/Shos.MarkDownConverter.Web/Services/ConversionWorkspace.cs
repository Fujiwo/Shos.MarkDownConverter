namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// 1 回の変換処理で使う一時ディレクトリと入出力ファイルの場所をまとめて管理します。
/// </summary>
public sealed class ConversionWorkspace : IDisposable
{
	private readonly ILogger _logger;
	private bool _disposed;

	private ConversionWorkspace(string workingDirectory, string inputPath, string outputPath, ILogger logger)
	{
		WorkingDirectory = workingDirectory;
		InputPath = inputPath;
		OutputPath = outputPath;
		_logger = logger;
	}

	public string WorkingDirectory { get; }

	public string InputPath { get; }

	public string OutputPath { get; }

	public static ConversionWorkspace Create(string? workingDirectoryRoot, string extension, ILogger logger)
	{
		// 親ディレクトリを差し替えられるようにしつつ、未指定時は OS 標準の一時領域へ逃がす。
		var root = string.IsNullOrWhiteSpace(workingDirectoryRoot)
			? Path.GetTempPath()
			: workingDirectoryRoot;
		var workingDirectory = Path.Combine(root, $"shos-markdownconverter-{Guid.NewGuid():N}");

		Directory.CreateDirectory(workingDirectory);

		return new ConversionWorkspace(
			workingDirectory,
			Path.Combine(workingDirectory, $"input{extension}"),
			Path.Combine(workingDirectory, "output.md"),
			logger);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		try
		{
			if (Directory.Exists(WorkingDirectory))
			{
				// 変換結果は永続保存しないため、処理後は作業ディレクトリごと削除する。
				Directory.Delete(WorkingDirectory, recursive: true);
			}
		}
		catch (Exception cleanupException)
		{
			_logger.LogWarning(cleanupException, "Failed to clean temporary directory {WorkingDirectory}", WorkingDirectory);
		}

		_disposed = true;
	}
}