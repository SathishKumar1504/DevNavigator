namespace DevNavigator.Api.DTOs;

public class SearchResultDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTime LastModified { get; set; }

    public int Score { get; set; }

    public List<string> MatchReasons { get; set; } = new();

    public List<SearchMatchDto> Matches { get; set; } = new();
}

public class SearchMatchDto
{
    public int LineNumber { get; set; }

    public string Line { get; set; } = string.Empty;

    public List<string> Context { get; set; } = new();
}