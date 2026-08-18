namespace JobWize.Frontend.Shared.Api
{
    internal sealed class RequestDescriptor
    {
        public string Url { get; init; } = string.Empty;

        public Dictionary<string, object?>? Body { get; init; }
    }
}
