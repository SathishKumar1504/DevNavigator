namespace DevNavigator.Api.Models;

public class CodeSymbolRelationship
{
    public int Id { get; set; }

    public int FromSymbolId { get; set; }

    public int ToSymbolId { get; set; }

    public string RelationshipType { get; set; } = string.Empty;

    public CodeSymbol? FromSymbol { get; set; }

    public CodeSymbol? ToSymbol { get; set; }
}