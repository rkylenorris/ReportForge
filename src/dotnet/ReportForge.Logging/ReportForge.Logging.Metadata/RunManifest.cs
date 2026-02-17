using System.Text;

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
    // TODO: Decide method for comparing current hash to previous or expected.
    public string SchemaHash { get; set; }

    public DatasetMetric(string datasetName, long rowCount, double queryDurationMs, string output, string schemaHash)
    {
        Name = datasetName;
        RowCount = rowCount;
        DurationMs = queryDurationMs;
        ParquetPath = output;
        SchemaHash = schemaHash;
    }
}
