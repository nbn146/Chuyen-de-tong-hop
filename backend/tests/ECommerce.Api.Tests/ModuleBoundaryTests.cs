using System.Text.RegularExpressions;

namespace ECommerce.Api.Tests;

/// <summary>
/// Architecture rules for the modular monolith. They scan the source tree so they need no extra
/// packages. A module may be reached from outside only through its Contracts (and its Entities,
/// which exist for the shared EF model). Other modules' Services, Controllers, Dtos and Persistence
/// are internal.
/// </summary>
public sealed partial class ModuleBoundaryTests
{
    private static readonly string ApiRoot = FindApiRoot();

    /// <summary>
    /// Which module owns which DbSet. Reading or writing another module's DbSet directly is a
    /// boundary breach, except for the documented, shrinking allow-list below.
    /// </summary>
    private static readonly Dictionary<string, string> DbSetOwner = new()
    {
        ["Roles"] = "Identity", ["Users"] = "Identity", ["RefreshTokens"] = "Identity",
        ["PasswordResetTokens"] = "Identity", ["Addresses"] = "Identity",
        ["Categories"] = "Catalog", ["Brands"] = "Catalog", ["Products"] = "Catalog", ["ProductImages"] = "Catalog",
        ["InventoryTransactions"] = "Inventory",
        ["Carts"] = "ShoppingCart", ["CartItems"] = "ShoppingCart",
        ["Orders"] = "Orders", ["OrderDetails"] = "Orders", ["OrderStatusHistories"] = "Orders",
        ["AiKnowledgeChunks"] = "AI", ["AiIndexJobs"] = "AI", ["AiChatSessions"] = "AI",
        ["AiChatMessages"] = "AI", ["AiMessageSources"] = "AI",
    };

    /// <summary>
    /// Known debt. Each entry is "ConsumerModule->DbSet". Remove an entry when the dependency is
    /// replaced by a contract; the test fails if an entry is no longer needed.
    /// </summary>
    private static readonly HashSet<string> AllowedDbSetAccess =
    [
        "Admin->Orders",          // ReportService read model
        "Admin->OrderDetails",    // ReportService read model
        "Admin->Products",        // ReportService read model
        "Admin->Users",           // ReportService read model
    ];

    [Fact]
    public void ModulesDoNotUseInternalsOfOtherModules()
    {
        var declared = ModuleFiles()
            .SelectMany(f => TypeNames(f.Text).Select(t => (f.Module, f.Folder, Type: t)))
            .Where(x => x.Folder is not ("Entities" or "Contracts"))
            .GroupBy(x => x.Type)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Module).Distinct().ToArray());

        var violations = new List<string>();
        foreach (var file in ModuleFiles())
        {
            foreach (var identifier in Identifiers(file.Text))
            {
                if (declared.TryGetValue(identifier, out var owners) &&
                    owners.Length == 1 && owners[0] != file.Module)
                {
                    violations.Add($"{file.Module}/{file.Name} uses {identifier} (internal to {owners[0]}) - expose it through {owners[0]}/Contracts");
                }
            }
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations.Distinct()));
    }

    [Fact]
    public void ModulesDoNotAccessOtherModulesDbSetsOutsideTheAllowList()
    {
        var used = FindForeignDbSetAccess();
        var violations = used.Except(AllowedDbSetAccess).OrderBy(x => x).ToList();

        Assert.True(violations.Count == 0,
            "Cross-module DbSet access (use a Contracts interface instead): " + string.Join(", ", violations));
    }

    [Fact]
    public void DbSetAllowListHasNoStaleEntries()
    {
        var stale = AllowedDbSetAccess.Except(FindForeignDbSetAccess()).OrderBy(x => x).ToList();

        Assert.True(stale.Count == 0, "Remove from AllowedDbSetAccess: " + string.Join(", ", stale));
    }

    [Fact]
    public void SharedKernelDoesNotDependOnModules()
    {
        var moduleTypes = ModuleFiles().SelectMany(f => TypeNames(f.Text)).ToHashSet();
        var violations = new List<string>();

        foreach (var path in Directory.EnumerateFiles(Path.Combine(ApiRoot, "Shared"), "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path);
            violations.AddRange(Identifiers(text)
                .Where(moduleTypes.Contains)
                .Select(i => $"{Path.GetFileName(path)} uses module type {i}"));
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations.Distinct()));
    }

    [Fact]
    public void EveryModuleExposesItsContractsOnlyThroughInterfacesAndRecords()
    {
        var offenders = ModuleFiles()
            .Where(f => f.Folder == "Contracts")
            .Where(f => ClassDeclaration().IsMatch(StripCommentsAndStrings(f.Text)))
            .Select(f => $"{f.Module}/Contracts/{f.Name}")
            .Where(n => !n.EndsWith("OrderConstants.cs") && !n.EndsWith("Dtos.cs") && !n.EndsWith("Dto.cs"))
            .ToList();

        Assert.True(offenders.Count == 0,
            "Contracts should be interfaces, records or DTOs, not services: " + string.Join(", ", offenders));
    }

    private static HashSet<string> FindForeignDbSetAccess()
    {
        var result = new HashSet<string>();
        foreach (var file in ModuleFiles())
        {
            foreach (Match match in DbSetAccess().Matches(StripCommentsAndStrings(file.Text)))
            {
                var dbSet = match.Groups[1].Value;
                if (DbSetOwner.TryGetValue(dbSet, out var owner) && owner != file.Module)
                {
                    result.Add($"{file.Module}->{dbSet}");
                }
            }
        }

        return result;
    }

    private sealed record ModuleFile(string Module, string Folder, string Name, string Text);

    private static List<ModuleFile> ModuleFiles()
    {
        var modulesRoot = Path.Combine(ApiRoot, "Modules");
        var files = new List<ModuleFile>();

        foreach (var path in Directory.EnumerateFiles(modulesRoot, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(modulesRoot, path).Replace('\\', '/').Split('/');
            var folder = relative.Length > 2 ? relative[1] : "Root";
            files.Add(new ModuleFile(relative[0], folder, Path.GetFileName(path), File.ReadAllText(path)));
        }

        return files;
    }

    private static IEnumerable<string> TypeNames(string text) =>
        TypeDeclaration().Matches(StripCommentsAndStrings(text)).Select(m => m.Groups[1].Value);

    private static IEnumerable<string> Identifiers(string text) =>
        IdentifierToken().Matches(StripCommentsAndStrings(text)).Select(m => m.Value).Distinct();

    private static string StripCommentsAndStrings(string text) => CommentsAndStrings().Replace(text, " ");

    private static string FindApiRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ECommerce.Api");
            if (Directory.Exists(Path.Combine(candidate, "Modules")))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/ECommerce.Api from the test output folder.");
    }

    [GeneratedRegex(@"\b(?:class|record(?:\s+struct)?|struct|interface|enum)\s+([A-Za-z_]\w*)")]
    private static partial Regex TypeDeclaration();

    [GeneratedRegex(@"\b[A-Za-z_]\w*\b")]
    private static partial Regex IdentifierToken();

    [GeneratedRegex(@"//[^\n]*|/\*.*?\*/|\$?@?""(?:[^""\\\n]|\\.)*""", RegexOptions.Singleline)]
    private static partial Regex CommentsAndStrings();

    [GeneratedRegex(@"\b\w*(?:[cC]ontext|[dD]b)\.([A-Z]\w*)\b")]
    private static partial Regex DbSetAccess();

    [GeneratedRegex(@"\b(?:public|internal)\s+(?:sealed\s+|static\s+|abstract\s+)*class\s")]
    private static partial Regex ClassDeclaration();
}
