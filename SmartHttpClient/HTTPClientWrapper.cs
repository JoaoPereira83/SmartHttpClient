

using SmartHttpClient.Enum;
using SmartHttpClient.Helpers;
using SmartHttpClient.Models;
using SmartHttpClient.Utility;

using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SmartHttpClient;

public class HTTPClientWrapper(IHttpClientFactory httpClientFactory) : IHTTPClientWrapper
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Regular expression to remove leading and trailing non-alphanumeric characters (optional underscores).
    /// </summary>
    private static readonly Regex _regex = new Regex(@"^[\W_]+|[\W_]+$", RegexOptions.Compiled);
    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Sends an HTTP request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
    /// <param name="request">The HTTP request details.</param>
    /// <returns>The deserialized response.</returns>
    public async Task<T> SendAsync<T>(HTTPClientRequest request)
    {
        try
        {
            using var httpClient = BuildDefaultHeaders(request);

            httpClient.Timeout = GetTimeout(request, httpClient);

            using var requestMessage = BuildRequestMessage(request);

            var response = await httpClient.SendAsync(requestMessage);
      
            if(typeof(T) == typeof(HttpResponseMessage))
            {
                return (T)(object)response;
            }

            await HandleErrorsAsync(response);

            var output = await DeserializeResponseContentAsync<T>(response);
            return output;
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            throw new HttpTimeoutException("Request timed out.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }



    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Sends an HTTP request and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
    /// <param name="request">The HTTP request details.</param>
    /// <returns>The deserialized response.</returns>
    public async Task SendAsync(HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));

        try
        {
            using var httpClient = BuildDefaultHeaders(request);

            httpClient.Timeout = GetTimeout(request, httpClient);

            using var requestMessage = BuildRequestMessage(request);

            var response = await httpClient.SendAsync(requestMessage);

            await HandleErrorsAsync(response);
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            throw new HttpTimeoutException("Request timed out.");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    // ----------------------------------------------------------------------------------------

    ///<summary>
    /// Deserializes the response content of an HTTP request.
    ///</summary>
    /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
    /// <param name="httpRequestMessage">The HttpResponseMessage object.</param>
    /// <returns>The deserialized response.</returns>
    private static async Task<T> DeserializeResponseContentAsync<T>(HttpResponseMessage httpRequestMessage)
    {
        ArgumentNullException.ThrowIfNull(httpRequestMessage, nameof(HttpResponseMessage));

        if (httpRequestMessage.Content is null)
        {
            throw new Exception("The HTTP request content is null.");
        }

        if (IsJsonContentType(httpRequestMessage.Content))
        {
            return await DeserializeJsonContentAsync<T>(httpRequestMessage);
        }

        if (HasContentDisposition(httpRequestMessage.Content))
        {
            return await HandleFileResponseAsync<T>(httpRequestMessage);
        }

        return default!;
    }

    /// <summary>
    /// Gets the timeout value for the HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request details.</param>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <returns>The timeout value for the HTTP request.</returns>
    private static TimeSpan GetTimeout(HTTPClientRequest request, HttpClient httpClient)
    {
        return request.Timeout != default ? request.Timeout : httpClient.Timeout;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Deserializes the JSON content of the HttpResponseMessage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpRequestMessage"></param>
    /// <returns></returns>
    private static async Task<T> DeserializeJsonContentAsync<T>(HttpResponseMessage httpRequestMessage)
    {
        await using var stream = await httpRequestMessage.Content.ReadAsStreamAsync();
        if (stream is null || stream.Length == 0)
        {
            return default!;
        }

        return await JsonSerializer.DeserializeAsync<T>(stream, BuildSerializerSettings()) ?? default!;
    }


    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Handles the file response based on the HttpResponseMessage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpRequestMessage"></param>
    /// <returns></returns>
    private static async Task<T> HandleFileResponseAsync<T>(HttpResponseMessage httpRequestMessage)
    {
        var fileResponse = await GetFileResponseAsync(httpRequestMessage);
        return (T)(object)fileResponse;
    }


    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Retrieves the file response from the HttpResponseMessage.
    /// </summary>
    /// <param name="httpRequestMessage">The HttpResponseMessage object.</param>
    /// <returns>The file response.</returns>
    private static async Task<FileResponse> GetFileResponseAsync(HttpResponseMessage httpRequestMessage)
    {
        ArgumentNullException.ThrowIfNull(httpRequestMessage, nameof(HttpResponseMessage));

        if (httpRequestMessage.Content is null)
        {
            throw new Exception("The HTTP request content is null.");
        }

        var fileName = TrimQuotes(httpRequestMessage.Content.Headers.ContentDisposition!.FileName!);
        var responseBytes = await httpRequestMessage.Content.ReadAsByteArrayAsync();
        var file = new FileResponse { FileName = fileName, FileContent = responseBytes };

        return file;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Trims the quotes from the file name.
    /// </summary>
    /// <param name="fileName">The file name to trim.</param>
    /// <returns>The trimmed file name.</returns>
    private static string TrimQuotes(string fileName)
    {
        // Return early if the input is null or empty
        if (string.IsNullOrEmpty(fileName))
        {
            return fileName;
        }

        // Remove leading and trailing quotes and non-alphanumeric characters (optional underscores)		
        return _regex.Replace(fileName, "");
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Checks if the HTTP content has a content disposition header.
    /// </summary>
    /// <param name="httpContent">The HTTP content to check.</param>
    /// <returns>True if the HTTP content has a content disposition header, false otherwise.</returns>
    private static bool HasContentDisposition(HttpContent httpContent)
    {
        ArgumentNullException.ThrowIfNull(httpContent, nameof(HttpContent));
        return httpContent.Headers.ContentDisposition is not null;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Checks if the HTTP content has a JSON media type.
    /// </summary>
    /// <param name="httpContent">The HttpContent object.</param>
    /// <returns>True if the HTTP content has a JSON media type, false otherwise.</returns>
    private static bool IsJsonContentType(HttpContent httpContent)
    {
        ArgumentNullException.ThrowIfNull(httpContent, nameof(HttpContent));
        var mediaType = httpContent.Headers.ContentType;
        return mediaType?.MediaType == MediaTypeNames.Application.Json;
    }

    // ----------------------------------------------------------------------------------------

    ///<summary>
    /// Builds and configures an instance of HttpClient with default headers.
    /// </summary>
    /// <param name="request">The HTTP request details.</param>
    /// <returns>The configured instance of HttpClient.</returns>
    private HttpClient BuildDefaultHeaders(HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));

        var httpClient = CreateHttpClient();

        ConfigureDefaultHeaders(httpClient);
        AddCustomHeaders(httpClient, request);

        AuthenticateRequest(httpClient, request);

        return httpClient;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Creates an instance of HttpClient.
    /// </summary>
    /// <returns></returns>
    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        return client;
    }

    // ----------------------------------------------------------------------------------------


    /// <summary>
    /// Configures the default headers of the HttpClient.
    /// </summary>
    /// <param name="client"></param>
    private static void ConfigureDefaultHeaders(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(HttpClient));

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }


    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// Adds custom headers to the HttpClient based on the provided HTTPClientRequest.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    private static void AddCustomHeaders(HttpClient client, HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));
        ArgumentNullException.ThrowIfNull(client, nameof(HttpClient));

        if (request.Headers is not null)
        {
            foreach (var header in request.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Authenticates the HTTP request based on the provided authorization details.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    private static void AuthenticateRequest(HttpClient client, HTTPClientRequest request)
    {
        // Assuming AuthenticateRequest logic mutates the HttpClient.
        _ = AuthenticateRequest(request, client);
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Builds and configures an instance of HttpRequestMessage based on the provided HTTPClientRequest.
    /// </summary>
    /// <param name="request">The HTTPClientRequest object.</param>
    /// <returns>The configured instance of HttpRequestMessage.</returns>
    private static HttpRequestMessage BuildRequestMessage(HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));

        var requestMessage = new HttpRequestMessage
        {
            Method = request.Method,
            RequestUri = BuildEndpointUri(request),
        };

        AddRequestBody(requestMessage, request);
        AddHttpContent(requestMessage, request);

        return requestMessage;
    }

    /// <summary>
    /// Adds the request body to the HttpRequestMessage based on the provided HTTPClientRequest.
    /// </summary>
    /// <param name="requestMessage"></param>
    /// <param name="request"></param>
    private static void AddRequestBody(HttpRequestMessage requestMessage, HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));
        ArgumentNullException.ThrowIfNull(requestMessage, nameof(HttpRequestMessage));

        if (request.RequestBody is not null)
        {
            requestMessage = SetRequestBody(requestMessage, request);
        }
    }


    /// <summary>
    /// Adds the HttpContent to the HttpRequestMessage based on the provided HTTPClientRequest.
    /// </summary>
    /// <param name="requestMessage"></param>
    /// <param name="request"></param>
    private static void AddHttpContent(HttpRequestMessage requestMessage, HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));
        ArgumentNullException.ThrowIfNull(requestMessage, nameof(HttpRequestMessage));

        if (request.HttpContent is not null)
        {
            requestMessage.Content = request.HttpContent;
        }
    }


    /// <summary>
    /// Sets the request body of the HttpRequestMessage based on the provided HTTPClientRequest.
    /// </summary>
    /// <param name="requestMessage"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    private static HttpRequestMessage SetRequestBody(HttpRequestMessage requestMessage, HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));
        ArgumentNullException.ThrowIfNull(requestMessage, nameof(HttpRequestMessage));

        if (request.RequestBody is not null)
        {
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(request.RequestBody,
                BuildSerializerSettings()))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json),
                }
            };
        }
        return requestMessage;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Authenticates the HTTP request based on the provided authorization details.
    /// </summary>
    /// <param name="request">The HTTPClientRequest object.</param>
    /// <param name="httpClient">The instance of HttpClient.</param>
    /// <returns>The authenticated instance of HttpClient.</returns>
    private static HttpClient AuthenticateRequest(HTTPClientRequest request, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));
        ArgumentNullException.ThrowIfNull(httpClient, nameof(HttpClient));

        if (request.Authenticator is null)
        {
            return httpClient;
        }

        httpClient.DefaultRequestHeaders.Authorization = request.Authenticator.Auth switch
        {
            AuthenticationMethod.None => null,
            AuthenticationMethod.Basic => new AuthenticationHeaderValue(
                               scheme: AuthenticationMethod.Basic.ToString(),
                                              EncodeAndDecodeBase64.Base64Encode($"{request.Authenticator.Username}:{request.Authenticator.Password}")),
            AuthenticationMethod.Bearer => new AuthenticationHeaderValue(scheme: AuthenticationMethod.Bearer.ToString(), request.Authenticator.AccessToken),
            AuthenticationMethod.ApiKey => new AuthenticationHeaderValue(scheme: AuthenticationMethod.ApiKey.ToString(), request.Authenticator.ApiKey),
            _ => throw new Exception("Invalid authorization type.")
        };

        return httpClient;
    }

    // ----------------------------------------------------------------------------------------

    ///<summary>
    /// Builds and configures an instance of JsonSerializerOptions with the specified settings.
    /// </summary>
    /// <returns>The configured instance of JsonSerializerOptions.</returns>
    private static JsonSerializerOptions BuildSerializerSettings()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Constructs the endpoint URL by adding query parameters to the provided URI.
    /// </summary>
    /// <param name="request">The HTTPClientRequest object.</param>
    /// <returns>The constructed endpoint URI.</returns>
    private static Uri BuildEndpointUri(HTTPClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(HTTPClientRequest));

        var endpointUri = request.BaseUri;

        if (request.EndpointParams is not null)
        {
            return new Uri(Query.AddQueryString(endpointUri.ToString(),
                                                       request.EndpointParams.QueryParams()!));
        }

        return endpointUri;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Handles the error response based on the media type header of the HttpResponseMessage.
    /// </summary>
    /// <param name="response">The HttpResponseMessage object.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static async Task HandleErrorsAsync(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));

        if (response.IsSuccessStatusCode)
            return;

        string responseContent = await response.Content.ReadAsStringAsync();

        // Determine content type
        string? mediaType = response.Content?.Headers?.ContentType?.MediaType;

        if (mediaType == MediaTypeNames.Application.Json)
        {
            var errorDetails = TryParseJsonErrors(responseContent);
            throw new ApiException(errorDetails, response.StatusCode);
        }

        if (mediaType == MediaTypeNames.Text.Plain)
        {
            throw new ApiException(responseContent, response.StatusCode);
        }

        // Fallback for non-JSON/non-plain responses
        string fallbackMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {responseContent}";
        throw new ApiException(fallbackMessage, response.StatusCode);
    }

    private static string TryParseJsonErrors(string jsonContent)
    {
        try
        {
            var problems = JsonSerializer.Deserialize<List<Problem>>(jsonContent, BuildSerializerSettings());
            return problems is not null
                ? string.Join(Environment.NewLine, problems.Select(p => p.Detail))
                : "Unknown error in JSON response.";
        }
        catch (JsonException)
        {
            return "Invalid JSON error format.";
        }
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Checks if the media type header of the HttpContent is "text/plain".
    /// </summary>
    /// <param name="httpContent">The HttpContent object.</param>
    /// <returns>True if the media type is "text/plain", false otherwise.</returns>
    private static bool IsMediaTypeHeaderValueTextPlain(HttpContent httpContent)
    {
        ArgumentNullException.ThrowIfNull(httpContent, nameof(HttpContent));

        var mediaType = httpContent.Headers.ContentType;

        return httpContent.Headers.ContentType?.MediaType == MediaTypeNames.Text.Plain;
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Checks if the media type header of the HttpContent is "text/html".
    /// </summary>
    /// <param name="httpContent">The HttpContent object.</param>
    /// <returns>True if the media type is "text/html", false otherwise.</returns>
    private static bool IsMediaTypeHeaderValueTextHtml(HttpContent httpContent)
    {
        ArgumentNullException.ThrowIfNull(httpContent, nameof(HttpContent));

        var mediaType = httpContent.Headers.ContentType;

        return httpContent.Headers.ContentType?.MediaType == MediaTypeNames.Text.Html;

    }
}

