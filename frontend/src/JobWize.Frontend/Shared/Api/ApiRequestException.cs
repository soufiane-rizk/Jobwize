namespace JobWize.Frontend.Shared.Api
{
    public sealed class ApiRequestException : Exception
    {
        public string HttpMethod { get; }

        public string Route { get; }

        public ApiRequestException(string httpMethod, string route, string message, Exception innerException)
            : base(message, innerException)
        {
            HttpMethod = httpMethod;
            Route = route;
        }
    }
}
