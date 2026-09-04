using System.ComponentModel.DataAnnotations;

namespace DevNavigator.Api.Models;

public class Repository
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RootPath { get; set; } = string.Empty;

    public DateTime AddedAt { get; set; }

    public DateTime? LastIndexedAt { get; set; }

    public int FileCount { get; set; }

    public ICollection<FileMetadata> Files { get; set; } = new List<FileMetadata>();
}