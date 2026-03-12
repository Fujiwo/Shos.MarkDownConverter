namespace Shos.MarkDownConverter.Web.Models;

public sealed record ErrorResponse(string Message, IReadOnlyList<string> Tips);