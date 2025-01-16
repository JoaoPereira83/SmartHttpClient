using System.Net;

namespace SmartHttpClient.Utility;
/// <summary>
/// Represents an exception that occurs during API calls.
/// </summary>
public class ApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    /// <summary>
    /// Gets or sets the HTTP status code associated with the exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}
