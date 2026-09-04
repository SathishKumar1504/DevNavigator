using System.Text.RegularExpressions;
using DevNavigator.Api.Models;

namespace DevNavigator.Api.Services;

public class CodeSymbolExtractor
{
    public List<CodeSymbol> Extract(
    int fileId,
    string content)
    {
        var symbols = new List<CodeSymbol>();

        ExtractImports(
            fileId,
            content,
            symbols);

        ExtractComponents(
            fileId,
            content,
            symbols);

        ExtractFunctions(
            fileId,
            content,
            symbols);

        ExtractExports(
            fileId,
            content,
            symbols);

        ExtractCSharpSymbols(
            fileId,
            content,
            symbols);

        ExtractCSharpDependencies(
            fileId,
            content,
            symbols);

        ExtractCSharpMethodCalls(
            fileId,
            content,
            symbols);

        return symbols;
    }

    private static void ExtractExports(
    int fileId,
    string content,
    List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // export { default } from "./ManagePayrollContainer";
            var exportFromMatch = Regex.Match(
                line,
                @"export\s+\{([^}]+)\}\s+from\s+[""']([^""']+)[""']");

            if (exportFromMatch.Success)
            {
                var names = exportFromMatch.Groups[1].Value
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                foreach (var name in names)
                {
                    symbols.Add(new CodeSymbol
                    {
                        FileId = fileId,
                        SymbolType = "Export",
                        Name = name,
                        LineNumber = i + 1,
                        ImportPath = exportFromMatch.Groups[2].Value
                    });
                }

                continue;
            }

            // export { Something };
            var exportMatch = Regex.Match(
                line,
                @"export\s*\{([^}]+)\}");

            if (exportMatch.Success)
            {
                var names = exportMatch.Groups[1].Value
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                foreach (var name in names)
                {
                    symbols.Add(new CodeSymbol
                    {
                        FileId = fileId,
                        SymbolType = "Export",
                        Name = name,
                        LineNumber = i + 1
                    });
                }

                continue;
            }

            // export default Something;
            var defaultExportMatch = Regex.Match(
                line,
                @"export\s+default\s+([A-Za-z_$][\w$]*)");

            if (defaultExportMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Export",
                    Name = defaultExportMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }
        }
    }

    private static void ExtractImports(
        int fileId,
        string content,
        List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Example:
            // import ManagePayrollContainer from "@/containers/Payroll/ManagePayrollContainer";

            var defaultImportMatch = Regex.Match(
                line,
                @"import\s+([A-Za-z_$][\w$]*)\s+from\s+[""']([^""']+)[""']");

            if (defaultImportMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Import",
                    Name = defaultImportMatch.Groups[1].Value,
                    LineNumber = i + 1,
                    ImportPath = defaultImportMatch.Groups[2].Value
                });

                continue;
            }

            // Example:
            // import { useState, useEffect } from "react";

            var namedImportMatch = Regex.Match(
                line,
                @"import\s+\{([^}]+)\}\s+from\s+[""']([^""']+)[""']");

            if (namedImportMatch.Success)
            {
                var names = namedImportMatch.Groups[1].Value
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                foreach (var name in names)
                {
                    symbols.Add(new CodeSymbol
                    {
                        FileId = fileId,
                        SymbolType = "Import",
                        Name = name,
                        LineNumber = i + 1,
                        ImportPath = namedImportMatch.Groups[2].Value
                    });
                }
            }
        }
    }

    private static void ExtractComponents(
        int fileId,
        string content,
        List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // const ManagePayroll = (...) =>
            var arrowComponentMatch = Regex.Match(
                line,
                @"(?:const|let|var)\s+([A-Z][A-Za-z0-9_$]*)\s*=\s*(?:\([^)]*\)|[A-Za-z_$][\w$]*)\s*=>");

            if (arrowComponentMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Component",
                    Name = arrowComponentMatch.Groups[1].Value,
                    LineNumber = i + 1
                });

                continue;
            }

            // function ManagePayroll(...)
            var functionComponentMatch = Regex.Match(
                line,
                @"function\s+([A-Z][A-Za-z0-9_$]*)\s*\(");

            if (functionComponentMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Component",
                    Name = functionComponentMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }
        }
    }

    private static void ExtractFunctions(
        int fileId,
        string content,
        List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // function getSomething(...)
            var functionMatch = Regex.Match(
                line,
                @"function\s+([a-z][A-Za-z0-9_$]*)\s*\(");

            if (functionMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Function",
                    Name = functionMatch.Groups[1].Value,
                    LineNumber = i + 1
                });

                continue;
            }

            // const getSomething = (...) =>
            var arrowFunctionMatch = Regex.Match(
                line,
                @"(?:const|let|var)\s+([a-z][A-Za-z0-9_$]*)\s*=\s*(?:\([^)]*\)|[A-Za-z_$][\w$]*)\s*=>");

            if (arrowFunctionMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Function",
                    Name = arrowFunctionMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }
        }
    }
    private static void ExtractCSharpSymbols(
    int fileId,
    string content,
    List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // public class PayrollController : ControllerBase
            var classMatch = Regex.Match(
                line,
                @"(?:public\s+|internal\s+|private\s+)?(?:partial\s+)?class\s+([A-Za-z_][A-Za-z0-9_]*)");

            if (classMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Class",
                    Name = classMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }

            // public interface GetFileLockInformationCommand
            var interfaceMatch = Regex.Match(
                line,
                @"(?:public\s+|internal\s+|private\s+)?interface\s+([A-Za-z_][A-Za-z0-9_]*)");

            if (interfaceMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Interface",
                    Name = interfaceMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }

            // public async Task<ActionResult<...>> GetFileLockInfo(...)
            // public IActionResult GetSomething(...)
            var methodMatch = Regex.Match(
                line,
                @"(?:public|private|protected|internal)\s+(?:(?:static|async|virtual|override|abstract|sealed|new)\s+)*[\w<>\[\],?.]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(");

            if (methodMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Method",
                    Name = methodMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }

            // public class XConsumer : IConsumer<T>
            var consumerMatch = Regex.Match(
                line,
                @"class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*IConsumer<");

            if (consumerMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Consumer",
                    Name = consumerMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }
        }
    }
    private static void ExtractCSharpDependencies(
    int fileId,
    string content,
    List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // IConsumer<GetFileLockInformationCommand>
            var consumerMatch = Regex.Match(
                line,
                @"IConsumer<\s*([A-Za-z_][A-Za-z0-9_.]*)\s*>");

            if (consumerMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Message",
                    Name = consumerMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }

            // IRequestClient<GetFileLockInformationCommand>
            var requestClientMatch = Regex.Match(
                line,
                @"IRequestClient<\s*([A-Za-z_][A-Za-z0-9_.]*)\s*>");

            if (requestClientMatch.Success)
            {
                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Message",
                    Name = requestClientMatch.Groups[1].Value,
                    LineNumber = i + 1
                });
            }
        }
    }
    private static void ExtractCSharpMethodCalls(
    int fileId,
    string content,
    List<CodeSymbol> symbols)
    {
        var lines = content.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Examples:
            //
            // _uploadedFilesRepository.GetFileLockInformation(
            //     context.Message.FileId);
            //
            // _logger.BeginScope(...);
            // context.RespondAsync(...);

            var callMatches = Regex.Matches(
                line,
                @"([A-Za-z_][A-Za-z0-9_]*)\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\(");

            foreach (Match callMatch in callMatches)
            {
                var receiver = callMatch.Groups[1].Value;
                var methodName = callMatch.Groups[2].Value;
    //            Console.WriteLine(
    //$"CALL FOUND: {receiver}.{methodName} at line {i + 1}");

                symbols.Add(new CodeSymbol
                {
                    FileId = fileId,
                    SymbolType = "Call",
                    Name = methodName,
                    LineNumber = i + 1,
                    ImportPath = receiver
                });
            }
        }
    }
}