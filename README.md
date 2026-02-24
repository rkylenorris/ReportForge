# Report Forge

> *in initial development*

> ***shift in project direction, changing version to 1.5 and moving towards SQLite and an Orchestrator functionality. README content will be updated soon -R. Kyle Norris 2/24/2026***

## Summary

`ReportForge` V1.0 is a PowerShell module for creating and automating sql-based, excel reporting. The module leverages the easy interoperability of PowerShell and C#, using some third-party C# libraries within a custom created C# DLL to create the majority of the module's functionality. PowerShell serves as a CLI wrapper for the classes from the custom DLL; end-users do not interact with these C# objects directly, instead they utilize the PowerShell module public Cmdlets.

The module is designed to create reports using data gathered via SQL queries and custom-made Excel templates. This allows for easy report styling and branding. The module maps the data to the appropriate places within the template using user-provided configuration values like anchor points or named ranges.

In addition to producing Excel reports, the module will also be able to save the SQL datasets as parquet files for easy import into tools like Power BI or Tableau.

### Proposed Command Interface

- The module will expose public commands for running reports and testing/debugging:

    1. **Invoke-RFRun**: Main entry point. Validates config, runs queries, generates Excel, and exports Parquet/Manifests. Should include a -DryRun switch parameter for running the report without publishing/distributing, placing resultant files in the test folder within that report's directory.
    2. **Test-RFConfig**: Parses config, resolves environment variables, and validates schema, returning a boolean value and logging any errors or conflicts.
    3. **Import-RFConfig**: Parses config, resolves environment variables, and validates schema. Returns a normalized config object.
    4. **Invoke-RFExcel**: Debug command to execute only the Excel generation step from in-memory data.  
    5. **Invoke-RFExport**: Debug command to execute only the Parquet/Manifest export step.

- The module will also expose public commands to aide in report development:

    1. **New-RFReport**: Similar to a project `init` command, this command creates the report directory and copies over all necessary requirements and placeholder files to be customized.
    2. **New-RFConfig**: Creates the report configuration `.yaml` file, using provided arguments to populate the module's config template.
    3. **Set-RFModuleConfig**: Sets Module level configuration values.
    4. **Register-RFTemplate**: For default-type report templates, to create similarly designed reports for different customers, products, or topics. Templates are copied to the Module level config's templates path property and their name and ID added to a Module registry (`json` or `sqlite` for V1.0).

⬆️ ***The above do not represent the module's functionality in its entirety.***

## Contributing

As this is the project's initial development, I am not accepting contributions at this time.

## Author

**R. Kyle Norris**: Lead Developer and Project Manager - [Github Profile](https://github.com/rkylenorris)

## License

[MIT](./LICENSE)
  