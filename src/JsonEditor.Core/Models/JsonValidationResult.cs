namespace JsonEditor.Core.Models;

public sealed class JsonValidationResult
{
    public static JsonValidationResult Valid() => new(true, null, null, null);

    public static JsonValidationResult Invalid(string errorMessage) => new(false, errorMessage, null, null);

    public static JsonValidationResult Invalid(string errorMessage, long? lineNumber, long? bytePositionInLine)
        => new(false, errorMessage, lineNumber, bytePositionInLine);

    private JsonValidationResult(bool isValid, string? errorMessage, long? lineNumber, long? bytePositionInLine)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        LineNumber = lineNumber;
        BytePositionInLine = bytePositionInLine;
    }

    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    public long? LineNumber { get; }

    public long? BytePositionInLine { get; }
}
