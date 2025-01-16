namespace SmartHttpClient.Helpers;

/// <summary>
/// Provides extension methods for string manipulation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts the first character of a string to lowercase.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>The string with the first character converted to lowercase.</returns>
    public static string FirstCharToLowerCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
        {
            return string.Concat(str[0].ToString().ToLower(), str.AsSpan(1));
        }

        return str;
    }
}
