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

// ...existing code...
public partial class EnvFile
{
    public string Path { get; private set; }
    public System.Collections.Generic.IDictionary<string, object> Variables { get; private set; }

    [GeneratedRegex(@"^\s*([^=]+)\s*=\s*(.+)\s*$", RegexOptions.None, "en-US")]
    private static partial Regex KeyValRegex();

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

            var m = KeyValRegex().Match(keyVal);
            if (m.Success)
            {
                var key = m.Groups[1].Value.Trim();
                var value = m.Groups[2].Value.Trim().Trim('"', '\'');
                dict[key] = ConvertString.ToCorrectType(value)!;
            }
        }

        return dict;
    }
}

public class EnvGlobals
{
    public static System.Collections.Generic.Dictionary<string, string?> Variables { get; } = Environment.GetEnvironmentVariables()
        .Cast<System.Collections.DictionaryEntry>()
        .ToDictionary(e => (string)e.Key!, e => e.Value as string);

}

public class Env
{

}
