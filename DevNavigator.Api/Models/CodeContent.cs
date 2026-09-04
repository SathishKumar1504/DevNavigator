using System.ComponentModel.DataAnnotations;

namespace DevNavigator.Api.Models;

public class CodeContent
{
    [Key]
    public int Id { get; set; }

    public int FileId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string ContentHash { get; set; } = string.Empty;

    public DateTime IndexedAt { get; set; }

    public FileMetadata? File { get; set; }
}