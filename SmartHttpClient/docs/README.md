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
dotnet add package SmartHttpClient --version 1.0.4
```

---

## **Setup**

### 1. **Register Services**
Add the following services in your `Program.cs` file to enable dependency injection for `SmartHttpClient`:

```csharp
builder.Services.AddHttpClient();
builder.Services.AddTransient<IHTTPClientWrapper, HTTPClientWrapper>();
```
---

## **Usage Examples**

# Product Service CRUD Operations

This service demonstrates how to perform CRUD (Create, Read, Update, Delete) operations using an `HTTPClientWrapper` with a strongly-typed `Product` class.

## Features

### 2. **Strongly-Typed Request (`T`)**
Leverage the generic `T` parameter to deserialize HTTP responses automatically into strongly-typed objects, such as the `Product` class.

---
### 3. **Dependency Injection Example**
To use SmartHttpClient, inject IHTTPClientWrapper into your services, controllers, or classes:
```csharp
public class ProductService(IHTTPClientWrapper hTTPClientWrapper)
{
    private readonly IHTTPClientWrapper hTTPClientWrapper = hTTPClientWrapper; 
}
```

## CRUD Operations

### 4. **Get a Single Product by ID**
Retrieve a specific product using its unique ID.  
**Example Use Case:** Fetch detailed information about a specific product.
```csharp
// Get a single product by ID
public async Task<Product> GetItemAsync(int id)
{
    var request = new HTTPClientRequest
    {
        BaseUri = new Uri("https://api.example.com/Product/{id}"),
        Method = HttpMethod.Get,
        Authenticator = new Authenticator
        {
            AccessToken = "TOKEN-ABC1234",
            Auth = AuthenticationMethod.Bearer
        }
    };

    return await _httpClientWrapper.SendAsync<Product>(request);
}   

```

### 5. **Get a List of Products**
Fetch all available products in the system.  
**Example Use Case:** Display all products in a catalog or inventory list.
```csharp
    // Get a list of products
    public async Task<IEnumerable<Product>> GetItemsAsync()
    {
        var request = new HTTPClientRequest
        {
            BaseUri = new Uri("https://api.example.com/Product/GetItems"),
            Method = HttpMethod.Get,
            Authenticator = new Authenticator
            {
                AccessToken = "TOKEN-ABC1234",
                Auth = AuthenticationMethod.Bearer
            }
        };

        return await _httpClientWrapper.SendAsync<IEnumerable<Product>>(request);
    }

```

### 6. **Create a New Product**
Add a new product to the system.  
**Example Use Case:** Allow users to add new items to an inventory.

```csharp
   // Create a new product
    public async Task<Product> CreateItemAsync(Product product)
    {
        var request = new HTTPClientRequest
        {
            BaseUri = new Uri("https://api.example.com/Product"),
            Method = HttpMethod.Post,
            RequestBody = product,
            Authenticator = new Authenticator
            {
                AccessToken = "TOKEN-ABC1234",
                Auth = AuthenticationMethod.Bearer
            }
        };

        return await _httpClientWrapper.SendAsync<Product>(request);
    }

```

### 7. **Update an Existing Product**
Modify details of an existing product by its ID.  
**Example Use Case:** Update the price, name, or description of a product.

```csharp
   // Update an existing product
    public async Task<Product> UpdateItemAsync(Product product)
    {
        var request = new HTTPClientRequest
        {
            BaseUri = new Uri("https://api.example.com/Product"),
            Method = HttpMethod.Put,
            RequestBody = product,
            Authenticator = new Authenticator
            {
                AccessToken = "TOKEN-ABC1234",
                Auth = AuthenticationMethod.Bearer
            }
        };

        return await _httpClientWrapper.SendAsync<Product>(request);
    }

```

### 8. **Delete a Product by ID**
Remove a product from the system using its unique ID.  
**Example Use Case:** Remove discontinued or obsolete products.

```csharp
     // Delete a product by ID
    public async Task<bool> DeleteItemAsync(int id)
    {
        var request = new HTTPClientRequest
        {
           BaseUri = new Uri("https://api.example.com/Product/{Id}"),
            Method = HttpMethod.Delete,
            Authenticator = new Authenticator
            {
                AccessToken = "TOKEN-ABC1234",
                Auth = AuthenticationMethod.Bearer
            }
        };

        var response = await _httpClientWrapper.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

```
---

### 9. **Downloading a File**
To handle file downloads, use the HTTPClientWrapper to make a GET request and save the file content locally.


### 10. **File Response Class**
**Note:** The `FileResponse` class is a custom class that contains the file content and metadata.
```csharp
public class FileResponse
{
    public string FileName { get; set; } // Name of the file
    public byte[] Content { get; set; } // File content as a byte array
}
```


### 11. **Downloading File**
**Example Use Case:** Download a file from a remote server and save it to the local filesystem.
```csharp
public async Task<FileResponse> DownloadFileAsync(int fileId)
{
    var request = new HTTPClientRequest
    {
        BaseUri = new Uri($"{https://api.example.com/FileDownload}{fileId}"),
        Method = HttpMethod.Get,
        Authenticator = new Authenticator
        {
            AccessToken = _sessionUser.AccessToken,
            Auth = AuthenticationMethod.Bearer
        }
    };

    var response = await _httpClientWrapper.SendAsync<FileResponse>(request);

    // Save file locally
    if (response != null && response.Content != null)
    {
        string filePath = Path.Combine("Downloads", response.FileName);
        await File.WriteAllBytesAsync(filePath, response.Content);
        Console.WriteLine($"File downloaded successfully to: {filePath}");
    }

    return response;
}
```
---

### 12. **Get Product with a Request**
This section demonstrates how to fetch products by passing additional parameters to the controller, such as filtering options or other product data.


### 13. **ProductRequest Class**
The ProductRequest class encapsulates all the parameters required for requesting product data from the API.
```csharp
public class ProductRequest
{  
    public required string Manufacture { get; init; }  
    public required string Model { get; init; }
    public required string Colour { get; init; }   
    public required decimal Price { get; init; }
    public required bool Imported { get; init; }
}

```

### 14. **Fetching Products with ProductRequest**
Here’s how you can use the ProductRequest class to pass parameters to the controller:
```csharp
public async Task<IEnumerable<Product>> GetProductAsync(ProductRequest productRequest)
{
    var request = new HTTPClientRequest
    {
        BaseUri = new Uri("https://api.example.com/Product/GetItems"),
        Method = HttpMethod.Get,
        EndpointParams = productRequest, // Pass ProductRequest as query parameters
        Authenticator = new Authenticator
        {
            AccessToken = _sessionUser.AccessToken,
            Auth = AuthenticationMethod.Bearer
        }
    };

    return await _httpClientWrapper.SendAsync<IEnumerable<Product>>(request);
}

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
This project is licensed under the **MIT License**. See the [LICENSE](https://github.com/JoaoPereira83/SmartHttpClient/blob/master/License) file for details.

---

## **Why Use SmartHttpClient?**
With **SmartHttpClient**, you no longer need to:
- Create and manage `HttpClient` instances.
- Write repetitive boilerplate for JSON serialization/deserialization.
- Manually handle authentication and headers.

**SmartHttpClient** is built to save developers time and effort, making HTTP requests clean, efficient, and easy to manage.

---
