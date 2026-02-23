using System.Data;
using System.Security.Cryptography;
using System.Text;


namespace ReportForge.Logging.Metadata;

// Row for data_dictionary.csv
public class DataDictionaryEntry
{
    public string DatasetName { get; set; }
    public string ColumnName { get; set; }
    public string DataType { get; set; } // e.g., "Decimal", "String"
    public bool IsNullable { get; set; }

    public DataDictionaryEntry(string dataSetName, string colName, string dType, bool isNullable)
    {
        DatasetName = dataSetName;
        ColumnName = colName;
        DataType = dType;
        IsNullable = isNullable;
    }

}

public static class DataDictionary
{
    private static List<DataDictionaryEntry> GetDataTableSchema(string dataSetName, DataTable table)
    {
        List<DataDictionaryEntry> entries = new(table.Columns.Count);

        foreach (DataColumn col in table.Columns)
        {
            DataDictionaryEntry entry = new(dataSetName.Trim(), col.ColumnName.Trim(), col.DataType.ToString().Trim(), col.AllowDBNull);

            entries.Add(entry);
        }

        return entries;
    }

    private static string GetSchemaHash(string schemaCsvString)
    {
        string schemaHash = string.Empty;

        if (string.IsNullOrEmpty(schemaCsvString))
        {
            return schemaHash;
        }

        byte[] textData = Encoding.UTF8.GetBytes(schemaCsvString);
        byte[] hash = SHA256.HashData(textData);

        schemaHash = Convert.ToHexString(hash);

        return schemaHash;
    }

    public static void WriteSchema<DataDictionaryEntry>(List<DataDictionaryEntry> dataDictionary, string outputDir)
    {
        if (!Path.Exists(outputDir))
        {
            throw new FileNotFoundException(string.Format("'{0}' directory does not exist.", outputDir));
        }

        StringBuilder csvString = new();
        string csvPath = Path.Join(outputDir, "data_dictionary.csv");
        string schemaHashPath = Path.Join(outputDir, "schema_hash.txt");
        if (Path.Exists(csvPath))
        {
            File.Delete(csvPath);
        }
        File.Create(csvPath);

        string headerRow = string.Empty;

        var schemaProperties = typeof(DataDictionaryEntry).GetProperties();

        foreach (var prop in schemaProperties)
        {
            headerRow += prop.Name.Trim() + "|";
        }

        csvString.AppendLine(headerRow.Substring(0, headerRow.Length - 2).Trim());

        foreach (DataDictionaryEntry entry in dataDictionary)
        {
            string line = string.Empty;
            foreach (var col in schemaProperties)
            {
                line += col.GetValue(entry, null) + "|";
            }
            string newLine = line.Substring(0, line.Length - 2);

            csvString.AppendLine(newLine.Trim());
        }

        string csvText = csvString.ToString();

        csvString.Clear();

        string schemaHash = GetSchemaHash(csvText);

        File.WriteAllText(schemaHashPath, schemaHash, Encoding.UTF8);

        StreamWriter csvWriter = new(csvPath, false, Encoding.UTF8);

        csvWriter.Write(csvText);
        csvWriter.Close();
    }
}