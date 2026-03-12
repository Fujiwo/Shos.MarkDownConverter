namespace Shos.MarkDownConverter.Web.Models;

public sealed record UiOptionsResponse(IReadOnlyList<string> AllowedExtensions, long MaxUploadSizeBytes);