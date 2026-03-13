namespace Shos.MarkDownConverter.Web.Services;

public static class FileSizeFormatter
{
	public static string Format(long bytes)
	{
		if (bytes < 1024)
		{
			return $"{bytes} B";
		}

		var kiloBytes = bytes / 1024d;
		if (kiloBytes < 1024)
		{
			return $"{kiloBytes:0.#} KB";
		}

		var megaBytes = kiloBytes / 1024d;
		return $"{megaBytes:0.#} MB";
	}
}