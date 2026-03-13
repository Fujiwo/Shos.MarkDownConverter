namespace Shos.MarkDownConverter.Web.Services;

public static class PayloadTooLargeDetector
{
	public static bool IsPayloadTooLarge(Exception? exception)
	{
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