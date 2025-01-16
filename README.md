# **SmartHttpClient**

**SmartHttpClient** is a powerful library for .NET 8.0 that simplifies working with `HttpClient`. It abstracts away repetitive tasks like creating new `HttpClient` instances, handling JSON serialization/deserialization, and managing headers, authentication, and timeouts. 

With **SmartHttpClient**, developers no longer need to manually handle JSON (it does it automatically for you). The library is flexible and supports both strongly typed (`T`) and non-typed requests.

---

## **Features**
- Simplifies `HttpClient` usage.
- Automatically serializes and deserializes JSON requests and responses.
- Supports dependency injection for easy integration.
- Includes built-in support for authentication (e.g., Bearer tokens).
- Handles headers, request timeouts, and endpoint parameters effortlessly.
- Flexible API to support both strongly-typed (`T`) and non-typed calls.

---

## **Installation**
Install the package via NuGet:

```bash
dotnet add package SmartHttpClient --version 1.0.0
```

---

## **Setup**

### 1. **Register Services**
Add the following services in your `Program.cs` file to enable dependency injection for `SmartHttpClient`:

```csharp
builder.Services.AddHttpClient();
builder.Services.AddTransient<IHTTPClientWrapper, HTTPClientWrapper>();
```

### 2. **Dependency Injection Example**
To use `SmartHttpClient`, inject `IHTTPClientWrapper` into your services, controllers, or classes:

```csharp
namespace HTTPClientService.Library;

/// <summary>
/// Represents a service for sending HTTP requests.
/// </summary>
public class MyService
{
    private readonly IHTTPClientWrapper _httpClientWrapper;

    public MyService(IHTTPClientWrapper httpClientWrapper)
    {
        _httpClientWrapper = httpClientWrapper;
    }

    /// <summary>
    /// Sends an HTTP request asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="requestParam">The parameters for the endpoint.</param>
    /// <returns>The task representing the asynchronous operation, containing the response.</returns>
    public async Task<T> SendRequestAsync<T>(object requestParam) where T : class
    {
        var request = new HTTPClientRequest
        {
            BaseUri = new Uri("https://localhost:7081/api/Application"),
            Method = HttpMethod.Get,
            RequestBody = new { Id = 1 },
            EndpointParams = requestParam,
            Timeout = TimeSpan.FromSeconds(5),
            Authenticator = new Authenticator
            {
                AccessToken = "TOKEN-ABC1234",
                Auth = AuthenticationMethod.Bearer
            },
            Headers = new Dictionary<string, string> { { "api-version", "2" } }
        };

        return await _httpClientWrapper.SendAsync<T>(request);
    }
}
```

---

## **Usage Examples**

### 1. **Strongly-Typed Request (`T`)**
If you expect a specific response type, like a custom class `MyResponseModel`, you can use the generic `T` parameter to deserialize the response automatically:

```csharp
var requestParams = new { Name = "Test", Value = 42 };
var result = await _httpClientWrapper.SendAsync<MyResponseModel>(new HTTPClientRequest
{
    BaseUri = new Uri("https://api.example.com"),
    Method = HttpMethod.Post,
    RequestBody = requestParams,
    Headers = new Dictionary<string, string>
    {
        { "Authorization", "Bearer your-access-token" }
    }
});

// `result` is automatically deserialized into the MyResponseModel class.
Console.WriteLine(result.PropertyName);
```

---

### 2. **Non-Typed Request (No `T`)**
If you don't want to use a specific type for the response, you can retrieve the raw response as a string or a dynamic object:

```csharp
var result = await _httpClientWrapper.SendAsync<dynamic>(new HTTPClientRequest
{
    BaseUri = new Uri("https://api.example.com"),
    Method = HttpMethod.Get,
    Headers = new Dictionary<string, string>
    {
        { "api-version", "1.0" }
    }
});

// Access the dynamic response.
Console.WriteLine(result.SomeProperty);
```

---

### 3. **Request Without Returning a Response**
If you don’t need a response or just want to send a request, you can use `void`:

```csharp
await _httpClientWrapper.SendAsync(new HTTPClientRequest
{
    BaseUri = new Uri("https://api.example.com"),
    Method = HttpMethod.Delete,
    Headers = new Dictionary<string, string>
    {
        { "Authorization", "Bearer your-access-token" }
    }
});
```

---

## **Dependencies**
- **Microsoft.Extensions.Http (>= 8.0.1)**

---

## **Contributing**
We welcome contributions! Please feel free to:
- Submit pull requests.
- Report issues or request features in the [GitHub repository](https://github.com/JoaoPereira83/SmartHttpClient).

---

## **License**
This project is licensed under the **MIT License**. See the [LICENSE](https://github.com/JoaoPereira83/HTTPClientService/blob/main/LICENSE) file for details.

---

## **Why Use SmartHttpClient?**
With **SmartHttpClient**, you no longer need to:
- Create and manage `HttpClient` instances.
- Write repetitive boilerplate for JSON serialization/deserialization.
- Manually handle authentication and headers.

**SmartHttpClient** is built to save developers time and effort, making HTTP requests clean, efficient, and easy to manage.

---
