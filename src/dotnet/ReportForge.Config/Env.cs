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

public class EnvFile
{
    public string Path { get; }
    public IDictionary Variables { get; private set; }

    private Regex LinePattern = new Regex(@"^\s*([^=]+)\s*=\s*(.+)\s*$");
    public EnvFile(string path)
    {
        Path = path;
        this.Variables = Parse();
    }

    private System.Collections.IDictionary Parse()
    {
        var lines = EnvFileReader.Load(this.Path);
        var dict = new System.Collections.Hashtable();
        foreach (var line in lines)
        {
            var match = LinePattern.Match(line);
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                dict[key] = value;
            }
        }
        return dict;
    }
}

public class EnvGlobals
{
    public static System.Collections.IDictionary Variables { get; } = Environment.GetEnvironmentVariables();

}

public class Env
{

}
