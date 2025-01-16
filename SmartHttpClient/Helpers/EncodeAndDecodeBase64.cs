using System.Text;

namespace HTTPClientService.Library.Helpers;

public static class EncodeAndDecodeBase64
{
    /// <summary>
    /// Encodes the input text to Base64.
    /// </summary>
    /// <param name="inputText">The input text to encode.</param>
    /// <returns>The Base64 encoded string.</returns>
    public static string Base64Encode(string inputText)
    {
        var inputTextBytes = Encoding.UTF8.GetBytes(inputText);
        return Convert.ToBase64String(inputTextBytes);
    }

    // ----------------------------------------------------------------------------------------

    /// <summary>
    /// Decodes the Base64 encoded string.
    /// </summary>
    /// <param name="base64Encode">The Base64 encoded string to decode.</param>
    /// <returns>The decoded string.</returns>
    public static string Base64Decode(string base64Encode)
    {
        var base64DecodeBytes = Convert.FromBase64String(base64Encode);
        return Encoding.UTF8.GetString(base64DecodeBytes);
    }
}
