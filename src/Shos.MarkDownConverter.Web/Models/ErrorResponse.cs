namespace Shos.MarkDownConverter.Web.Models;

public sealed record ErrorResponse(
	string Code,
	string Message,
	IReadOnlyList<string> PossibleCauses,
	IReadOnlyList<string> Actions);