using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Core;



namespace ReportForge.Config;

public class EnvFileReader
{
    public static string[] Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Env file not found at path: {path}");
        }
        return File.ReadAllLines(path);
    }
}

public partial class EnvFileRegex
{
    [GeneratedRegex(@"^\s*([^=]+)\s*=\s*(.+)\s*$", RegexOptions.None, "en-US")]
    public static partial Regex KeyVal();
}

public class EnvFile
{
    public string Path { get; private set; }
    public System.Collections.Generic.IDictionary<string, object> Variables { get; private set; }

    public EnvFile(string path)
    {
        Path = path;
        Variables = Parse();
    }

    private System.Collections.Generic.Dictionary<string, object> Parse()
    {
        var lines = EnvFileReader.Load(this.Path);
        var dict = new System.Collections.Generic.Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

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
                var key = m.Groups[1].Value.Trim();
                var value = m.Groups[2].Value.Trim().Trim('"', '\'');
                dict[key] = ConvertEnvString.ToCorrectType(value)!;
            }
        }

        return dict;
    }
}

public class EnvGlobals
{
    public static System.Collections.Generic.Dictionary<string, object> Variables { get; } = Environment.GetEnvironmentVariables()
        .Cast<System.Collections.DictionaryEntry>()
        .ToDictionary(e => (string)e.Key!, e => ConvertEnvString.ToCorrectType((string)e.Value!)!);

}

public class ReportForgeEnv
{
    // TODO: Merge multiple env files and globals
}
