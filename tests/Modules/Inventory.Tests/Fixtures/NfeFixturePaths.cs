namespace Inventory.Tests.Fixtures;

/// <summary>
/// Resolves NF-e XML fixture paths for unit and integration tests.
/// </summary>
public static class NfeFixturePaths
{
    /// <summary>Returns absolute path to a fixture file under Fixtures/nfe.</summary>
    public static string Resolve(string fileName)
    {
        var fromOutput = Path.Combine(AppContext.BaseDirectory, "Fixtures", "nfe", fileName);
        if (File.Exists(fromOutput))
        {
            return fromOutput;
        }

        var fromRepo = Path.GetFullPath(Path.Combine("tests", "Modules", "Inventory.Tests", "Fixtures", "nfe", fileName));
        return fromRepo;
    }
}
