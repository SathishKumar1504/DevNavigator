namespace DevNavigator.Api.Models;

public class CodeSymbol
{
    public int Id { get; set; }

    public int FileId { get; set; }

    public string SymbolType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int LineNumber { get; set; }

    public string? ImportPath { get; set; }

    public FileMetadata? File { get; set; }
}