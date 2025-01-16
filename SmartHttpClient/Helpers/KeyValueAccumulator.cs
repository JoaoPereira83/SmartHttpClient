using Microsoft.Extensions.Primitives;

namespace HTTPClientService.Library.Helpers;

/// <summary>
/// A structure that accumulates key-value pairs, allowing multiple values for a single key.
/// </summary>
public struct KeyValueAccumulator
{
    /// <summary>
    /// Dictionary to store key-value pairs.
    /// </summary>
    private Dictionary<string, StringValues> _accumulator;
    /// <summary>
    /// Dictionary to store key-value pairs when values need to be expanded.
    /// </summary>
    private Dictionary<string, List<string>> _expandingAccumulator;
    /// <summary>
    /// Indicates whether the accumulator has any values.
    /// </summary>
    public readonly bool HasValues => ValueCount > 0;
    /// <summary>
    /// Gets the number of keys in the accumulator.
    /// </summary>
    public readonly int KeyCount => _accumulator?.Count ?? 0;
    /// <summary>
    /// Gets the total number of values in the accumulator.
    /// </summary>
    public int ValueCount { get; private set; }

    /// <summary>
    /// Appends a key-value pair to the accumulator. If the key already exists, the value is added to the existing values.
    /// </summary>
    /// <param name="key">The key to append.</param>
    /// <param name="value">The value to append.</param>
    public void Append(string key, string value)
    {
        _accumulator ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

        if (_accumulator.TryGetValue(key, out var value2))
        {
            if (value2.Count == 0)
            {
                _expandingAccumulator[key].Add(value);
            }
            else if (value2.Count == 1)
            {
                _accumulator[key] = new string[2]
                {
                        value2[0]!,
                        value
                };
            }
            else
            {
                _accumulator[key] = default;
                _expandingAccumulator ??= new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                List<string> list = new List<string>(8);
                string[] array = value2.ToArray()!;
                list.Add(array[0]);
                list.Add(array[1]);
                list.Add(value);
                _expandingAccumulator[key] = list;
            }
        }
        else
        {
            _accumulator[key] = new StringValues(value);
        }

        ValueCount++;
    }

    /// <summary>
    /// Gets the accumulated key-value pairs as a dictionary.
    /// </summary>
    /// <returns>A dictionary containing the accumulated key-value pairs.</returns>
    public readonly Dictionary<string, StringValues> GetResults()
    {
        if (_expandingAccumulator != null)
        {
            foreach (KeyValuePair<string, List<string>> item in _expandingAccumulator)
            {
                _accumulator[item.Key] = new StringValues(item.Value.ToArray());
            }
        }

        return _accumulator ?? new Dictionary<string, StringValues>(0, StringComparer.OrdinalIgnoreCase);
    }
}
