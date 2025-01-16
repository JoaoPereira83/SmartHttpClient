namespace HTTPClientService.Library;

/// <summary>
/// Represents an interface for an HTTP client service.
/// </summary>
public interface IHTTPClientWrapper
{
    /// <summary>
    /// Sends an HTTP request asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="request">The HTTP request to send.</param>
    /// <returns>The task representing the asynchronous operation, containing the response.</returns>
    Task<T> SendAsync<T>(HTTPClientRequest request);

    /// <summary>
    /// Sends an HTTP request asynchronously.
    /// </summary>
    /// <param name="request">The HTTP request to send.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task SendAsync(HTTPClientRequest request);
}
