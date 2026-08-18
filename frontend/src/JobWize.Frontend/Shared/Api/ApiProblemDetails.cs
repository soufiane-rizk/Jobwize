using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobWize.Frontend.Shared.Api
{
    internal sealed class ApiProblemDetails
    {
        public string? Title { get; init; }

        public string? Detail { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Extensions { get; init; }
    }
}
