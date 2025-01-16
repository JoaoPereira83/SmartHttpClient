using System.Net;

namespace SmartHttpClient.Utility;
/// <summary>
/// Exception thrown when an HTTP request times out.
/// </summary>
public class HttpTimeoutException(string message) : ApiException(message, HttpStatusCode.RequestTimeout) { }
