using DevNavigator.Api.Data;
using DevNavigator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Services;

public class CodeSymbolRelationshipBuilder
{
    private readonly AppDbContext _db;

    public CodeSymbolRelationshipBuilder(AppDbContext db)
    {
        _db = db;
    }

    public async Task BuildForRepositoryAsync(int repositoryId)
    {
        var existingRelationships = await _db.CodeSymbolRelationships
            .Where(x =>
                x.FromSymbol.File!.RepositoryId == repositoryId)
            .ToListAsync();

        if (existingRelationships.Count > 0)
        {
            _db.CodeSymbolRelationships.RemoveRange(
                existingRelationships);
        }

        var allSymbols = await _db.CodeSymbols
            .Include(x => x.File)
            .Where(x =>
                x.File!.RepositoryId == repositoryId)
            .ToListAsync();

        var symbolsByName = allSymbols
            .GroupBy(
                x => x.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.ToList(),
                StringComparer.OrdinalIgnoreCase);

        var files = allSymbols
            .Select(x => x.File!)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

        foreach (var file in files)
        {
            var symbols = allSymbols
                .Where(x => x.FileId == file.Id)
                .ToList();

            BuildForFile(
                symbols,
                symbolsByName);
        }

        await _db.SaveChangesAsync();
    }

    private void BuildForFile(
        List<CodeSymbol> symbols,
        Dictionary<string, List<CodeSymbol>> symbolsByName)
    {
        if (symbols.Count == 0)
            return;

        // ---------------------------------------------------------
        // 1. Consumer → Message
        // ---------------------------------------------------------

        var consumers = symbols
            .Where(x => x.SymbolType == "Consumer")
            .ToList();

        var messages = symbols
            .Where(x => x.SymbolType == "Message")
            .ToList();

        foreach (var consumer in consumers)
        {
            foreach (var message in messages)
            {
                AddRelationship(
                    consumer.Id,
                    message.Id,
                    "Consumes");
            }
        }

        // ---------------------------------------------------------
        // 1b. Consumer → Method
        //
        // GetFileLockInformationConsumer
        //       ↓
        // Consume
        // ---------------------------------------------------------

        foreach (var consumer in consumers)
        {
            var consumerMethod = symbols
                .Where(x =>
                    x.SymbolType == "Method" &&
                    x.Name == "Consume")
                .FirstOrDefault();

            if (consumerMethod == null)
                continue;

            AddRelationship(
                consumer.Id,
                consumerMethod.Id,
                "Contains");
        }

        // ---------------------------------------------------------
        // 2. Controller → Message
        // ---------------------------------------------------------

        var controllers = symbols
            .Where(x =>
                x.SymbolType == "Class" &&
                x.Name.EndsWith(
                    "Controller",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var controller in controllers)
        {
            foreach (var message in messages)
            {
                AddRelationship(
                    controller.Id,
                    message.Id,
                    "Requests");
            }
        }

        // ---------------------------------------------------------
        // 3. Method call relationship
        //
        // Example:
        //
        // Consume
        //    ↓ Calls
        // GetFileLockInformation
        //
        // Only create a relationship when the method can be
        // resolved uniquely. This prevents unrelated methods such
        // as Add(), BeginScope(), etc. from being selected.
        // ---------------------------------------------------------

        var methods = symbols
            .Where(x => x.SymbolType == "Method")
            .OrderBy(x => x.LineNumber)
            .ToList();

        var calls = symbols
            .Where(x => x.SymbolType == "Call")
            .OrderBy(x => x.LineNumber)
            .ToList();

        foreach (var call in calls)
        {
            if (!symbolsByName.TryGetValue(
                    call.Name,
                    out var candidates))
            {
                continue;
            }

            var methodCandidates = candidates
                .Where(x => x.SymbolType == "Method")
                .ToList();

            // We cannot safely determine the target when
            // multiple methods have the same name.
            if (methodCandidates.Count != 1)
            {
                continue;
            }

            var targetMethod = methodCandidates[0];

            // Find the method containing the call.
            var callerMethod = methods
                .Where(x =>
                    x.LineNumber < call.LineNumber)
                .OrderByDescending(x => x.LineNumber)
                .FirstOrDefault();

            if (callerMethod == null)
            {
                continue;
            }

            // Do not create a self relationship.
            if (callerMethod.Id == targetMethod.Id)
            {
                continue;
            }

            AddRelationship(
                callerMethod.Id,
                targetMethod.Id,
                "Calls");
        }
    }
    private void AddRelationship(
        int fromSymbolId,
        int toSymbolId,
        string relationshipType)
    {
        if (fromSymbolId == toSymbolId)
            return;

        _db.CodeSymbolRelationships.Add(
            new CodeSymbolRelationship
            {
                FromSymbolId = fromSymbolId,
                ToSymbolId = toSymbolId,
                RelationshipType = relationshipType
            });
    }


}