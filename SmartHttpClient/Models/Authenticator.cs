using HTTPClientService.Library.Enum;

namespace HTTPClientService.Library.Models;

public class Authenticator
{
    /// <summary>
    /// Gets or sets the authentication details.
    /// </summary>
    public AuthenticationMethod Auth { get; set; }

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the API key.
    /// </summary>
    public string? ApiKey { get; set; }
}
