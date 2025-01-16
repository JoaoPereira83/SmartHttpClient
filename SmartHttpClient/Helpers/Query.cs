using Microsoft.Extensions.Primitives;

using System.Text;
using System.Text.Encodings.Web;

namespace SmartHttpClient.Helpers;

public static class Query
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>The combined result</returns>

    public static string AddQueryString(string uri, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        return AddQueryString(uri,
        [
            new(name, value)
        ]);
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="queryString"></param>
    /// <returns></returns>

    public static string AddQueryString(string uri, IDictionary<string, string> queryString)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(queryString);

        return AddQueryString(uri, (IEnumerable<KeyValuePair<string, string>>)queryString);
    }

    /// <summary>
    /// Adds query string parameters to the given URI.
    /// </summary>
    /// <param name="uri">The base URI to which the query string will be added.</param>
    /// <param name="queryString">An enumerable collection of key-value pairs representing the query string parameters.</param>
    /// <returns>The URI with the added query string parameters.</returns>
    private static string AddQueryString(string uri, IEnumerable<KeyValuePair<string, string>> queryString)
    {
        ArgumentNullException.ThrowIfNull(uri);
        ArgumentNullException.ThrowIfNull(queryString);

        int num = uri.IndexOf('#');
        string text = uri;
        string value = "";

        if (num != -1)
        {
            value = uri[num..];
            text = uri[..num];
        }

        bool flag = text.IndexOf('?') != -1;

        var sb = new StringBuilder();
        sb.Append(text);

        foreach (KeyValuePair<string, string> item in queryString)
        {
            sb.Append(flag ? '&' : '?');
            sb.Append(UrlEncoder.Default.Encode(item.Key));
            sb.Append('=');
            sb.Append(UrlEncoder.Default.Encode(item.Value));
            flag = true;
        }

        sb.Append(value);
        return sb.ToString();
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Parse a query string into its component key and value parts.
    /// </summary>
    /// <param name="queryString">The raw query string value, with or without the leading '?'.</param>
    /// <returns>A collection of parsed keys and values.</returns>

    public static Dictionary<string, StringValues> ParseQuery(string queryString)
    {
        Dictionary<string, StringValues>? dictionary = ParseNullableQuery(queryString);
        if (dictionary == null)
        {
            return [];
        }

        return dictionary;
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Parse a query string into its component key and value parts.
    /// </summary>
    /// <param name="queryString">The raw query string value, with or without the leading '?'.</param>
    /// <returns>A collection of parsed keys and values, null if there are no entries.</returns>

    public static Dictionary<string, StringValues>? ParseNullableQuery(string queryString)
    {
        KeyValueAccumulator keyValueAccumulator = default;

        if (string.IsNullOrEmpty(queryString) || queryString == "?")
        {
            return null;
        }

        int i = 0;
        if (queryString[0] == '?')
        {
            i = 1;
        }

        int length = queryString.Length;
        int num = queryString.IndexOf('=');
        if (num == -1)
        {
            num = length;
        }

        while (i < length)
        {
            int num2 = queryString.IndexOf('&', i);
            if (num2 == -1)
            {
                num2 = length;
            }

            if (num < num2)
            {
                for (; i != num && char.IsWhiteSpace(queryString[i]); i++)
                {
                }

                string text = queryString[i..num];
                string text2 = queryString.Substring(num + 1, num2 - num - 1);
                keyValueAccumulator.Append(Uri.UnescapeDataString(text.Replace('+', ' ')), Uri.UnescapeDataString(text2.Replace('+', ' ')));
                num = queryString.IndexOf('=', num2);
                if (num == -1)
                {
                    num = length;
                }
            }
            else if (num2 > i)
            {
                keyValueAccumulator.Append(queryString[i..num2], string.Empty);
            }

            i = num2 + 1;
        }

        if (!keyValueAccumulator.HasValues)
        {
            return null;
        }

        return keyValueAccumulator.GetResults();
    }
}
