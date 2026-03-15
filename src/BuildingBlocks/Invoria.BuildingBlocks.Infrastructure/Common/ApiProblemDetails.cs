using System.Net;

namespace Invoria.BuildingBlocks.Infrastructure.Common
{
    public class ApiProblemDetails
    {
        public string Type { get; init; } = default!;
        public string Title { get; init; } = default!;
        public int Status { get; init; }
        public string? Detail { get; init; }
        public string? Instance { get; init; }
        public string? CorrelationId { get; init; }

        public string? ErrorCode { get; init; }
        public string? ErrorKey { get; init; }

        public Dictionary<string, string[]> Errors { get; init; } = new();

        public static ApiProblemDetails FromException(Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string? instance = null)
        {
            return new ApiProblemDetails
            {
                Type = "https://httpstatuses.io/" + (int)statusCode,
                Title = "An unexpected error occurred.",
                Status = (int)statusCode,
                Detail = exception.Message,
                Instance = instance
            };
        }
    }
}
