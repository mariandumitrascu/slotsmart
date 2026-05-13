namespace SlotSmart.Shared.Errors;

/// <summary>
/// Stable, transport-agnostic error value. Translated to RFC 7807 ProblemDetails at the API layer.
/// </summary>
/// <param name="Code">Machine-readable identifier, kebab-case (e.g. "member.not_found"). Stable across releases.</param>
/// <param name="Title">Short, human-readable summary.</param>
/// <param name="Detail">Optional longer human-readable explanation.</param>
/// <param name="Type">Optional URI categorising the error (RFC 7807 'type'). Defaults to "about:blank".</param>
public sealed record Error(string Code, string Title, string? Detail = null, string Type = "about:blank")
{
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>Common factory for resource-not-found errors.</summary>
    public static Error NotFound(string code, string what) =>
        new(code, "Resource not found", $"The requested {what} was not found.");

    /// <summary>Common factory for validation errors.</summary>
    public static Error Validation(string code, string detail) =>
        new(code, "Validation failed", detail);

    /// <summary>Common factory for conflict errors (e.g., duplicate, race condition).</summary>
    public static Error Conflict(string code, string detail) =>
        new(code, "Conflict", detail);

    /// <summary>Common factory for unauthorized errors.</summary>
    public static Error Unauthorized(string code, string detail) =>
        new(code, "Unauthorized", detail);

    public bool IsNone => string.IsNullOrEmpty(Code);
}
