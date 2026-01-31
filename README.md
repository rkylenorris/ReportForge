# Report Forge

> *in initial development*

## Summary

`ReportForge` is a PowerShell module for creating and automating sql-based, excel reporting. The module leverages the easy interoperability of PowerShell and C#, using some third-party C# libraries within a custom created C# DLL to create the majority of the module's functionality. PowerShell mainly serves as a wrapper for the classes imported from the custom DLL; end-users do not interact with these C# objects directly, instead they utilize the PowerShell module public Cmdlets.

The module is designed to create reports using data gathered via SQL queries and custom-made Excel templates. This allows for easy report styling and branding. The module maps the data to the appropriate places within the template using user-provided configuration values.
