# Report Forge Directory Structures

---

## Project Directory Structure

---

```bash
ReportForge/  
├── ReportForge.psd1           # Root Module Manifest  
├── ReportForge.psm1           # Root Module Loader 
# (loads DLLs + scripts)  
├── ReportForge.slnx           # C# Project Solution File
├── README.md                  # Project README including 
# summary, Contributing, Author, License, (eventually) Quick-Start
├── Documentation/             # Project documentation 
# in various formats (.md, .pdf)
├── Templates/                 # Stores templates for .yaml 
# config dev files, json files representing directory structures,
# README.md & CHANGELOG.md templates for reports
├── src/  
│   ├── dotnet/                # C# Class Library
│   └── powershell/            # PowerShell Source
├── Output/                    # Compiled Build Artifacts
│   └── ReportForge/  
│       ├── Assemblies/        # Compiled DLLs + Dependencies  
│       └── ...                # the .psd1 and .psm1 files & 
# the Public & Private folders
└── Tests/                     # Pester Tests, C# Tests
```

## Standard Development Report Directory Structure

---

```bash
ReportTitle/
├── .env                    # Secrets & local overrides
├── ReportTitle.yaml        # Report level config: stored in 
# yaml during development; stored in registry.db in production
├── README.md               # Documentation for the specific report
├── CHANGELOG.md            # Change log for the specific report 
# for each version change (1.0.0.1 -> 1.0.1.1 | 1.5.0.1 -> 2.0.0.1)
├── Datasets.schema         # Defined schema/types per current 
# dataset; pipe delimited flat file with .schema extension
├── SchemaHash.txt          # calculated hex file-hash for report's
# current datasets schema     
├── logs/                   # Log files
|   ├── ReportTitle.log     # Current log file (structured NDJSON)
|   └── Archive/            # Archived logs by month
|       ├── ReportTitle_{YYYY_MMM}.log
|       └── ReportTitle_{YYYY_MMM}.log
├── sql/                    # SQL source files (Version controlled)
│   └── SalesData.sql  
├── templates/              # Excel templates (ClosedXML targets)
│   └── Monthly_Report.xlsx
├── output/                 # Active run artifacts 
# (Parquet, Excel, Manifest)
|    ├── Current/
|    |    ├── ReportTitle.xlsx
|    |    └── RunArtifacts/
|    |         ├── Datasets_{date}.schema
|    |         ├── SchemaHash_{date}.txt
|    |         ├── Manifest_{date}.json
|    |         └── Parquet/ # Folder for Parquet exported
|    └── Archive/           # Previous report runs,
# keeps last 10 runs in dated folders
```

## Global Module Resources Directory

---

```bash
 %APPDATA%\ReportForge\ (Windows) or ~/.config/reportforge/ (Linux)
 ├── registry.db                # Centralized SQLite Registry
 ├── logging.config.json        # Default Logger Config
 ├── secrets.env                # Global-scope secrets
 └── Templates/                 # Standard & Branded report templates
     ├── BlankBrandedTemplate.xlsx
     ├── MonthlySales/
     └── FinanceAudit/
```

## Standard Production Report Directory Structure

---

```bash
ReportTitle/
├── .env                    # Secrets & local overrides
├── README.md               # Documentation for the specific report
├── CHANGELOG.md            # Change log for the specific report
# for each version change (1.0.0.1 -> 1.0.1.1 | 1.5.0.1 -> 2.0.0.1)
├── Datasets.schema         # Defined schema/types per current dataset;
# pipe delimited flat file with .schema extension
├── SchemaHash.txt          # calculated hex file-hash for 
#report datasets schema
├── logs/                   # Log files
|   ├── ReportTitle.log     # Current log file (structured NDJSON)
|   └── Archive/            # Archived logs by month
|       ├── ReportTitle_{YYYY_MMM}.log
|       └── ReportTitle_{YYYY_MMM}.log
├── sql/                    # SQL source files (Version controlled)
│   └── SalesData.sql
├── templates/              # Excel templates (ClosedXML targets)
│   └── Monthly_Report.xlsx
├── output/                 # Run output (Parquet, Excel, Manifest)
|    ├── Current/
|    |    ├── ReportTitle.xlsx
|    |    └── Parquet/      # Folder for Parquet exported datasets
|    └── Archive/           # Previous runs, in by month folders 
```
