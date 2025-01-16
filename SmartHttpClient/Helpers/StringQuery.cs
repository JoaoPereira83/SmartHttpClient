using System.Reflection;

namespace HTTPClientService.Library.Helpers;

public static class StringQuery
{
    /// <summary>
    /// Converts the properties of the specified object into a dictionary of query parameters.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="TItem">The object to convert.</param>
    /// <returns>A dictionary of query parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided object is null.</exception>
    public static Dictionary<string, string>? QueryParams<T>(this T TItem) where T : class
    {
        _ = TItem ?? throw new ArgumentNullException(nameof(TItem));

        var result = TItem.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name.FirstCharToLowerCase(), prop => Convert.ToString(prop.GetValue(TItem, null)));

        if (result is { })
        {
            foreach (var item in result)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    result.Remove(item.Key);
                }
            }

            return result!;
        }

        return null;
    }
}
