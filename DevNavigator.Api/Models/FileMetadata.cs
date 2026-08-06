using System.ComponentModel.DataAnnotations;

namespace DevNavigator.Api.Models;

public class FileMetadata
{
    [Key]
    public int Id { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string Folder { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTime LastModified { get; set; }
}