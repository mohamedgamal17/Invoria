namespace Invoria.BuildingBlocks.Infrastructure.OpenApi;

/// <summary>
/// Reusable short descriptions for OpenAPI <c>Summary.Responses</c> entries.
/// </summary>
public static class InvoriaOpenApiResponseDescriptions
{
    public const string Ok200 =
        "Success. HTTP 200. Envelope with isSuccess true and the operation result in result.";

    public const string BadRequest400 =
        "Bad request. Validation failed or malformed input. HTTP 400. Envelope with validation_error and field errors.";

    public const string Unauthorized401 =
        "Unauthorized. HTTP 401. Authentication required or token invalid.";

    public const string Forbidden403 =
        "Forbidden. HTTP 403. Caller is not allowed to perform this operation.";

    public const string NotFound404 =
        "Not found. HTTP 404. The requested resource does not exist.";

    public const string Conflict409 =
        "Conflict. HTTP 409. The request conflicts with the current state of the resource.";

    public const string UnprocessableEntity422 =
        "Unprocessable entity. HTTP 422. A business rule prevented the operation.";

    public const string InternalServerError500 =
        "Server error. HTTP 500. An unexpected error occurred.";
}
