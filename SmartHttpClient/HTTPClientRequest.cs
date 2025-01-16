using HTTPClientService.Library.Models;

namespace HTTPClientService.Library;

/// <summary>
/// Represents an HTTP client request.
/// </summary>
public class HTTPClientRequest
{
    /// <summary>
    /// Gets or sets the base URI of the request.
    /// </summary>
    public required Uri BaseUri { get; init; }

    /// <summary>
    /// Gets or sets the HTTP method of the request.
    /// </summary>
    public required HttpMethod Method { get; init; }

    /// <summary>
    /// Gets or sets the headers of the request.
    /// </summary>
    public IDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Gets or sets the authenticator for the request.
    /// </summary>
    public Authenticator? Authenticator { get; init; }

    /// <summary>
    /// Gets or sets the HTTP content of the request.
    /// </summary>
    public HttpContent? HttpContent { get; init; }

    /// <summary>
    /// Gets or sets the timeout of the request.
    /// </summary>
    public TimeSpan Timeout { get; init; } = default!;

    /// <summary>
    /// Gets or sets the request body of the request.
    /// </summary>
    public object? RequestBody { get; init; }

    /// <summary>
    /// Gets or sets the endpoint parameters of the request.
    /// </summary>
    public object? EndpointParams { get; init; }
}
