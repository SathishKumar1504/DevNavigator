using DevNavigator.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DevNavigator.Api.Services;

public class ServiceNavigationService
{
    private readonly AppDbContext _db;
    private readonly ImportResolver _resolver;

    public ServiceNavigationService(
        AppDbContext db,
        ImportResolver resolver)
    {
        _db = db;
        _resolver = resolver;
    }

    public async Task<object?> ResolveServiceAsync(
        int repositoryId,
        int fileId,
        string symbol)
    {
        var sourceFile = await _db.Files
            .FirstOrDefaultAsync(x =>
                x.Id == fileId &&
                x.RepositoryId == repositoryId);

        if (sourceFile == null)
            return null;

        // Find the import of the requested service function.
        var serviceImport = await _db.CodeSymbols
            .FirstOrDefaultAsync(x =>
                x.FileId == fileId &&
                x.SymbolType == "Import" &&
                x.Name == symbol &&
                x.ImportPath != null);

        if (serviceImport == null)
            return null;

        // Resolve:
        // ManagePayrollContainer.jsx
        //       ↓
        // @/services/payroll
        //       ↓
        // services/payroll/index.js
        var serviceFile = await _resolver.ResolveAsync(
            repositoryId,
            serviceImport.ImportPath!,
            sourceFile.RelativePath);

        if (serviceFile == null)
            return null;

        // Get service source.
        var serviceContent = await _db.CodeContents
            .FirstOrDefaultAsync(x =>
                x.FileId == serviceFile.Id);

        if (serviceContent == null)
            return null;

        // Find which endpoint module the service imports.
        var endpointImport = await _db.CodeSymbols
            .FirstOrDefaultAsync(x =>
                x.FileId == serviceFile.Id &&
                x.SymbolType == "Import" &&
                x.ImportPath != null &&
                x.ImportPath.StartsWith("@/services/endpoints/"));

        if (endpointImport == null)
            return null;

        // Resolve:
        // @/services/endpoints/payroll
        //       ↓
        // services/endpoints/payroll.js
        var endpointFile = await _resolver.ResolveAsync(
            repositoryId,
            endpointImport.ImportPath!,
            serviceFile.RelativePath);

        if (endpointFile == null)
            return null;

        // Read endpoint source.
        var endpointContent = await _db.CodeContents
            .FirstOrDefaultAsync(x =>
                x.FileId == endpointFile.Id);

        if (endpointContent == null)
            return null;

        // Find:
        // getFileLockInfo: "/api/v1/Payroll/GetFileLockInfo"
        var path = FindEndpointPath(
            endpointContent.Content,
            symbol);

        return new
        {
            symbol,

            source = new
            {
                fileId = sourceFile.Id,
                fileName = sourceFile.FileName,
                relativePath = sourceFile.RelativePath,
                lineNumber = serviceImport.LineNumber
            },

            service = new
            {
                fileId = serviceFile.Id,
                fileName = serviceFile.FileName,
                relativePath = serviceFile.RelativePath
            },

            endpoint = new
            {
                fileId = endpointFile.Id,
                fileName = endpointFile.FileName,
                relativePath = endpointFile.RelativePath,
                importPath = endpointImport.ImportPath,
                path
            }
        };
    }

    private static string? FindEndpointPath(
        string content,
        string symbol)
    {
        // Matches:
        //
        // getFileLockInfo: "/api/v1/Payroll/GetFileLockInfo"
        //
        // Also handles whitespace/newlines around the colon.

        var pattern =
            $@"\b{Regex.Escape(symbol)}\s*:\s*[""']([^""']+)[""']";

        var match = Regex.Match(
            content,
            pattern,
            RegexOptions.IgnoreCase);

        return match.Success
            ? match.Groups[1].Value
            : null;
    }
}