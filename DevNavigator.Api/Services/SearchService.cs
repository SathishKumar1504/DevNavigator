using DevNavigator.Api.Data;
using DevNavigator.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;

    public SearchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SearchResultDto>> SearchAsync(
        int repositoryId,
        string query)
    {
        var terms = query
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var files = await _db.Files
            .Include(x => x.CodeContent)
            .Where(x => x.RepositoryId == repositoryId)
            .ToListAsync();

        var results = new List<SearchResultDto>();

        foreach (var file in files)
        {
            var result = AnalyzeFile(file, terms);

            if (result != null)
            {
                results.Add(result);
            }
        }

        return results
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.FileName)
            .Take(100)
            .ToList();
    }

    private static SearchResultDto? AnalyzeFile(
    Models.FileMetadata file,
    List<string> terms)
    {
        var code = file.CodeContent?.Content ?? string.Empty;

        var lines = code.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        var score = 0;

        var reasons = new List<string>();

        var matches = new List<SearchMatchDto>();

        var matchedTerms = 0;

        foreach (var term in terms)
        {
            var termMatched = false;

            // -------------------------
            // Filename
            // -------------------------

            if (file.FileName.Equals(
                    term,
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 100;

                termMatched = true;

                reasons.Add(
                    $"Exact filename match: '{term}'");
            }
            else if (file.FileName.Contains(
                         term,
                         StringComparison.OrdinalIgnoreCase))
            {
                score += 70;

                termMatched = true;

                reasons.Add(
                    $"Filename contains: '{term}'");
            }

            // -------------------------
            // Path
            // -------------------------

            if (file.RelativePath.Contains(
                    term,
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 30;

                termMatched = true;

                reasons.Add(
                    $"Path contains: '{term}'");
            }

            // -------------------------
            // Source code
            // -------------------------

            var termLineIndexes = new List<int>();

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase))
                {
                    termMatched = true;

                    termLineIndexes.Add(i);
                }
            }

            if (termLineIndexes.Count > 0)
            {
                score += 20;

                score += Math.Min(
                    termLineIndexes.Count * 5,
                    25);

                reasons.Add(
                    $"Source code contains: '{term}'");

                foreach (var lineIndex in termLineIndexes)
                {
                    // Prevent duplicate lines when multiple
                    // search terms match the same line.
                    if (matches.Any(x =>
                            x.LineNumber == lineIndex + 1))
                    {
                        continue;
                    }

                    if (matches.Count >= 10)
                    {
                        break;
                    }

                    matches.Add(new SearchMatchDto
                    {
                        LineNumber = lineIndex + 1,
                        Line = lines[lineIndex].Trim(),
                        Context = GetContext(
                            lines,
                            lineIndex,
                            2)
                    });
                }
            }

            if (termMatched)
            {
                matchedTerms++;
            }
        }

        // Bonus for matching multiple search terms.
        score += matchedTerms * 25;

        // Nothing matched.
        if (matchedTerms == 0)
        {
            return null;
        }

        return new SearchResultDto
        {
            Id = file.Id,
            FileName = file.FileName,
            Extension = file.Extension,
            RelativePath = file.RelativePath,
            Size = file.Size,
            LastModified = file.LastModified,
            Score = score,
            MatchReasons = reasons
                .Distinct()
                .ToList(),
            Matches = matches
        };
    }

    private static List<string> GetContext(
        string[] lines,
        int lineIndex,
        int contextLines)
    {
        var start = Math.Max(
            0,
            lineIndex - contextLines);

        var end = Math.Min(
            lines.Length - 1,
            lineIndex + contextLines);

        var context = new List<string>();

        for (var i = start; i <= end; i++)
        {
            context.Add(
                $"{i + 1}: {lines[i].Trim()}");
        }

        return context;
    }
}