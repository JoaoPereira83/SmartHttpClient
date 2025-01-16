namespace SmartHttpClient.Models;
/// <summary>
/// Represents a response containing a file's name and its content.
/// </summary>
public class FileResponse
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string FileName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the content of the file as a byte array.
    /// </summary>
    public byte[] FileContent { get; set; } = default!;
}
