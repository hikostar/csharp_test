namespace JsonEditor.Core.Models;

public sealed class JsonValidationResult
{
    public static JsonValidationResult Valid() => new(true, null);

    public static JsonValidationResult Invalid(string errorMessage) => new(false, errorMessage);

    private JsonValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }
}
