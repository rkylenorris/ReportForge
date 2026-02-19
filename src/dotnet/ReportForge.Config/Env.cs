using System.Collections;
using System.Text.RegularExpressions;

namespace ReportForge.Config;

public partial class EnvFileRegex
{
    [GeneratedRegex(@"^\s*([^=]+)\s*=\s*(.+)\s*$", RegexOptions.None, "en-US")]
    public static partial Regex KeyVal();
}

public class EnvFile
{
    public string Path { get; private set; }
    public string Name { get; private set; }
    public string RunEnvironment { get; private set; }
    public Dictionary<string, object> Variables { get; private set; }

    public EnvFile(string path)
    {
        Path = VerifyPath(path);
        string[] pathParts = Path.Split("//");
        Name = pathParts[pathParts.Length - 1];
        RunEnvironment = GetRunEnvironmentFromFile();
        Variables = ParseFile();
    }

    private string VerifyPath(string path)
    {
        if (!File.Exists(Path))
        {
            throw new FileNotFoundException($"Env file not found at path: {Path}");
        }
        else
        {
            return path;
        }
    }

    private string GetRunEnvironmentFromFile()
    {
        string[] possibleEnvs = ["dev", "test", "prod"];
        string runEnv = string.Empty;

        if (Name.Contains('.'))
        {
            runEnv = Name.Split(".")[0];
            if (possibleEnvs.Contains<string>(runEnv))
            {
                return runEnv;
            }
            else
            {
                runEnv = "dev";
            }
        }

        return runEnv;
    }

    private Dictionary<string, object> ParseFile()
    {
        var lines = File.ReadAllLines(Path);
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue; // Skip empty lines and comments
            }

            string keyVal = line.Trim();

            var m = EnvFileRegex.KeyVal().Match(keyVal);
            if (m.Success)
            {
                string key = m.Groups[1].Value.Trim();
                string value = m.Groups[2].Value.Trim().Trim('"', '\'');
                dict[key] = ConvertEnvString.ToCorrectType(value)!;
            }
            else
            {
                Console.WriteLine($"Invalid entry in env file {Name}, entry: {keyVal}");
                // Change to log statement after logging namespace is finished.
            }
        }

        return dict;
    }
}

public class ModuleEnvFile : EnvFile
{
    public string Type = "module";

    public ModuleEnvFile(string path) : base(path)
    {

    }
}

public class ReportEnvFile : EnvFile
{
    public string Type = "report";

    public ReportEnvFile(string path) : base(path)
    {

    }
}

public static class EnvGlobals
{
    public static Dictionary<string, object> Variables { get; } = Environment.GetEnvironmentVariables()
        .Cast<DictionaryEntry>()
        .ToDictionary(e => (string)e.Key!, e => ConvertEnvString.ToCorrectType((string)e.Value!)!);

}

public class ReportForgeEnv
{
    public Dictionary<string, object> Variables { get; private set; }

    public ReportForgeEnv()
    {
        Variables = EnvGlobals.Variables;
    }

    public ReportForgeEnv(ModuleEnvFile moduleEnv)
    {
        Variables = EnvGlobals.Variables;

        foreach (var kvp in moduleEnv.Variables)
        {
            Variables[kvp.Key] = kvp.Value;
        }

    }

    public ReportForgeEnv(ModuleEnvFile moduleEnv, ReportEnvFile reportEnv)
    {
        Variables = EnvGlobals.Variables;

        foreach (var kvp in moduleEnv.Variables)
        {
            Variables[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in reportEnv.Variables)
        {
            Variables[kvp.Key] = kvp.Value;
        }

    }
}
