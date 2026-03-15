namespace Shos.MarkDownConverter.Web.Services;

/// <summary>
/// アップロード上限超過を示す例外を吸い上げ、実行環境差を隠して 1 つの判定にそろえます。
/// </summary>
public static class PayloadTooLargeDetector
{
	public static bool IsPayloadTooLarge(Exception? exception)
	{
		// 413 はホストや multipart 解析段階によって内側の例外へ包まれるため、連鎖をたどって判定する。
		for (var current = exception; current is not null; current = current.InnerException)
		{
			if (current is BadHttpRequestException badHttpRequestException
				&& badHttpRequestException.StatusCode == StatusCodes.Status413PayloadTooLarge)
			{
				return true;
			}

			if (current is InvalidDataException invalidDataException && ContainsPayloadTooLargeMessage(invalidDataException.Message))
			{
				return true;
			}

			if (current is BadHttpRequestException fallbackBadRequestException && ContainsPayloadTooLargeMessage(fallbackBadRequestException.Message))
			{
				return true;
			}
		}

		return false;
	}

	private static bool ContainsPayloadTooLargeMessage(string message)
	{
		return message.Contains("Multipart body length limit", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("Request body too large", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("body length limit", StringComparison.OrdinalIgnoreCase);
	}
}