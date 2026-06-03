using System.Text.Json;

namespace FindMyHobbyApi.Domain;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}
