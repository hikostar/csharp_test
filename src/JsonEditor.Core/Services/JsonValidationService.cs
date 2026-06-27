using System.Text.Json;
using JsonEditor.Core.Models;

namespace JsonEditor.Core.Services;

public sealed class JsonValidationService : IJsonValidationService
{
    public JsonValidationResult Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return JsonValidationResult.Valid();
        }

        try
        {
            using var _ = JsonDocument.Parse(text);
            return JsonValidationResult.Valid();
        }
        catch (JsonException ex)
        {
            return JsonValidationResult.Invalid(ex.Message, ex.LineNumber, ex.BytePositionInLine);
        }
    }
}
