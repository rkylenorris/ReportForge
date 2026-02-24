using System.Text;
using System.Text.RegularExpressions;

namespace ReportForge.Logging.Metadata;

public enum RFEnvironment
{
    DEV,
    TEST,
    PROD
}

public class ManifestRunTime
{
    private readonly string formatString = @"dd\:hh\:mm\:ss";
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; } = new();
    public TimeSpan Duration { get; private set; } = new();

    public ManifestRunTime()
    {
        Start = DateTime.Now;
    }

    public void SetRunDuration()
    {
        End = DateTime.Now;

        TimeSpan duration = End - Start;

        Duration = duration;
    }

    public override string ToString()
    {
        return Duration.ToString(formatString);
    }


}

// Root object for manifest.json
public class RunManifest
{
    public Guid RunId { get; private set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; private set; } = DateTime.UtcNow;
    public DateTime TimestampLocal { get; private set; }
    public string ReportTitle { get; private set; }
    public ManifestRunTime RunTime { get; private set; }
    public string User { get; private set; }
    public string Machine { get; private set; }
    public string Environment { get; private set; } // dev, test, prod
    public string ConfigPath { get; private set; }
    public bool Success { get; private set; } = true;

    // Detailed metrics for each dataset processed
    public List<DatasetMetric> Datasets { get; private set; } = new();

    // Capture validation warnings (e.g., "Column 'X' missing in template")
    public List<string> Warnings { get; private set; } = new();

    public RunManifest(string reportTitle, RFEnvironment environment, string configPath)
    {
        RunTime = new();
        TimestampLocal = TimestampUtc.ToLocalTime();
        ReportTitle = reportTitle;
        User = System.Environment.UserName;
        Machine = System.Environment.MachineName;
        Environment = environment.ToString().ToLower();
        ConfigPath = configPath;
    }

    private void UpdateTimestamps()
    {
        TimestampUtc = DateTime.UtcNow;
        TimestampLocal = TimestampUtc.ToLocalTime();
    }

    public void AddDataset(DatasetMetric dataset)
    {
        Datasets.Add(dataset);
        UpdateTimestamps();
    }

    public void AddWarning(string warningMessage)
    {
        Warnings.Add(warningMessage);
        UpdateTimestamps();
    }

}

// Details for a single dataset (part of Manifest)
public class DatasetMetric
{
    public string Name { get; set; }
    public long RowCount { get; set; }
    public double DurationMs { get; set; }
    public string ParquetPath { get; set; }

    // A hash of column names+types. 
    // Changes if SQL schema changes, alerting you to breaking changes.
    public string PreviousSchemaHash { get; private set; }
    public string SchemaHash { get; private set; }

    private string FileFriendlyName;

    public DatasetMetric(string datasetName, long rowCount, double queryDurationMs, string outputDir, string schemaHash)
    {
        Name = datasetName;
        FileFriendlyName = Regex.Replace(Name.Trim(), @"[^a-zA-Z0-9.\s_-]", "");
        RowCount = rowCount;
        DurationMs = queryDurationMs;
        ParquetPath = Path.Join(outputDir, string.Format("{0}.parquet", FileFriendlyName));
        SchemaHash = schemaHash; // TODO: refactor to include fact that need to get previous hash before new hash is created.
    }

    private string GetPreviousSchemaHash(string outputDir)
    {
        string hash = string.Empty;
        string hashPath = Path.Join(outputDir, "schema_hash.txt");
        DateTime lastHashWriteUtc = File.GetLastWriteTimeUtc(hashPath);
        DateTime lastHashWriteLocal = lastHashWriteUtc.ToLocalTime();
        hash = File.ReadAllText(hashPath, Encoding.UTF8).Trim('\n').Trim();

        string archivesPath = Path.Join(Environment.CurrentDirectory, "archives");

        if (!Path.Exists(archivesPath))
        {
            File.Create(archivesPath);
        }

        string archiveHashPath = Path.Join(archivesPath, string.Format("schema_hash_{0}.txt", lastHashWriteLocal.ToString("yyyy-MM-dd_HH-mm")));

        File.WriteAllText(archiveHashPath, hash);

        File.Delete(hashPath);

        return hash;
    }
}
