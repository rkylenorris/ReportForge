# ReportForge v1.0 – Technical Specification & Requirements

- **Version:** 1.0.0.1
- **Status:** Approved for Implementation
- **License:** MIT

## 1. Project Overview

ReportForge is a PowerShell-first reporting automation tool designed to generate repeatable, production-grade business reports from SQL Server data sources. It orchestrates the execution of SQL queries, populates Excel templates (preserving formatting and formulas) using ClosedXML, and exports Power BI–ready Parquet datasets with structured metadata.

### 1.1 Core Philosophy

- **Orchestration vs. Engine:** PowerShell handles UX, CLI, and flow control; C# (.NET 8) handles performance-critical logic (Excel/Parquet generation) and strict configuration parsing.  
- **Resume-Grade Quality:** Emphasizes strict typing, DTOs (Data Transfer Objects), validation, structured logging, and CI/CD over ad-hoc scripting.  
- **Fail-Fast:** Configuration and schema errors must stop execution before any data is written.

### 1.2 Scope & Non-Goals

**In Scope (v1):**

- SQL Server data sources (Integrated Auth \+ SQL Login via Environment Variables).  
- Excel generation using pre-defined .xlsx templates (ClosedXML).  
- Parquet export for Power BI ingestion.  
- Run metadata (Manifest) and Data Dictionary generation.  
- YAML-based configuration with Environment Variable interpolation.

**Out of Scope (v1):**

- Power BI Service publishing or REST API refresh (Planned for v2).  
- PBIX file generation.  
- Support for non-SQL Server sources (e.g., SQLite, CSV).  
- SecretManagement module integration (v1 uses .env files/env vars only).

## 2. System Architecture

### 2.1 Directory Structure

The project follows a strict separation between source code and build artifacts to ensure CI/CD compatibility.  

```bash
ReportForge/  
├── ReportForge.psd1           # Root Module Manifest  
├── ReportForge.psm1           # Root Module Loader (loads DLLs + scripts)  
├── src/  
│   ├── dotnet/                # C# Solution (Core, Excel, Parquet)  
│   │   ├── ReportForge.Core/  
│   │   ├── ReportForge.Excel/  
│   │   └── ReportForge.Parquet/  
│   └── powershell/            # PowerShell Source (Public/Private functions)  
├── Output/                    # Compiled Build Artifacts (Mirror of published module)  
│   └── ReportForge/  
│       ├── Assemblies/        # Compiled DLLs + Dependencies (ClosedXML, YamlDotNet)  
│       └── ...  
└── Tests/                     # Pester Tests
```

### 2.2 Technology Stack

- **Orchestration:** PowerShell 7+ (cross-platform compatible).  
- **Runtime:** .NET 10.  
- **Configuration Parsing:** C# using YamlDotNet (deserialized to strong DTOs)  
- **Excel Engine:** C# library wrapping ClosedXML.  
- **Parquet Engine:** C# library wrapping Parquet.Net.

## 3. Functional Requirements

### 3.1 Command Interface

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

### 3.2 Configuration & Secrets

- **Configuration**:  
  - Split into **Module Global Config** (user defaults) and **Report Specific Config** (per report)  
    - **Module Global Config**: Contains non-secret defaults and reusable connection aliases  
    - **Report Specific Config:** Contains fields for run, source, auth, datasets, excel, mappings, exports  
  - **Schema Requirements:**  
    - Format: Yaml  
    - Parsing: Handled in C# to map directly to DTOs.  
  - **Validation Rules:**  
    - Required fields: version, source, datasets, excel.template_path.  
    - Dataset uniqueness: Duplicate dataset names must throw an error.  
    - Path Resolution: All relative paths resolve relative to the *config file location*, not the current working directory.  
- **Secret Management (v1 Strategy):**
  - **No secrets in YAML.** Passwords/Connection Strings must use ${VAR_NAME} interpolation.  
  - **Resolution Precedence (Highest wins):**  
    - Report-local .env file (e.g., dev.env, .env).  
    - User-scope Module secrets (*%APPDATA%\\ReportForge\\secrets.env*).  
    - Process Environment Variables.  
  - **Safety:** Resolved secrets exist only in memory (C# SecureString or transient connection string) and are *never* logged.

### 3.3 Data Pipeline

- **Ingestion:** Execute SQL queries defined in config. Capture row counts and execution duration.  
- **Type Mapping:** Enforce strict C# -> Parquet type mapping (e.g., SQL decimal -> .NET decimal; SQL bit -> bool).  
- **Excel Output:**  
  - Open Template.  
  - Locate "Anchors" (Named Ranges or Cells).  
  - Write DataTable contents.  
  - *Constraint:* Preserve existing formulas and styles; do not blow away table definitions.  
- **Parquet Output:**  
  - Write one .parquet file per named dataset.  
  - Enforce stable schema (column names/types must match previous runs or configured expectations).  
- **Artifact Generation:**  
  - manifest.json: Run ID, timestamp, schema hashes, row counts.  
  - data_dictionary.csv: Dataset name, column name, data type.

## 4. Configuration Schema Specification

### 4.1 Report Config (report.yaml)

The source of truth for a specific run.  

```yaml
version: 1  
run:  
  name: "MonthlySales"  
  environment: "prod"         # dev | test | prod  
  output_root: "./output"  
  overwrite: false

source:  
  type: "sqlserver"  
  # Variables interpolated from .env or process env  
  server: "${RF_SQL_SERVER}"  
  database: "${RF_SQL_DB}"  
  auth:  
    mode: "env"               # integrated | env  
    username_env: "RF_SQL_USER"  
    password_env: "RF_SQL_PASS"

datasets:  
  - name: "SalesData"  
    query_file: "./sql/GetSales.sql"
    # OR query: "SELECT \* FROM Sales"

excel:  
  template_path: "./templates/SalesTemplate.xlsx"  
  output_name: "Sales_Report_${DATE}.xlsx"  
  mappings:  
    - dataset: "SalesData"  
      sheet: "Data_Input"  
      start_cell: "A2"  
      include_headers: false

exports:  
  enabled: true  
  format: "parquet"  
  dictionary: true
```

### 4.2 Module Config (module.config.yaml)

- Stored in `%APPDATA%\\ReportForge\\` (Windows) or `~/.config/reportforge/` (Linux).
- Contains non-secret defaults and reusable connection aliases.  

```yaml
# ReportForge \- module-level configuration (non-secret defaults \+ env references)  
# This file is intended to live in the user config home, e.g.:  
#   Windows: %APPDATA%\\ReportForge\\module.config.yaml  
#   Linux/macOS: ~/.config/reportforge/module.config.yaml  
#  
# Secrets should NOT be stored in this YAML.  
# Use a separate env file (recommended):  
#   Windows: %APPDATA%\\ReportForge\\secrets.env  
#   Linux/macOS: ~/.config/reportforge/secrets.env  
#  
# Env precedence (v1 recommended):  
#   Report env > Module secrets.env > Process env

version: 1

module:  
  # Human-friendly identity used in manifests/log headers; does not affect module name.  
  display_name: "Company Name - ReportForge"  
  # Optional: used for locating the user config home folder on some platforms.  
  config_folder_name: "CompanyNameReportForge"

branding:  
  # Optional metadata you can inject into exported artifacts / manifests.  
  organization: "Your Org"  
  product_line: "Reporting"  
  contact: "<data-team@your-org.example>"  
  # Optional: small image path or URL. 
  logo_ref: null

templates:  
  # Root folder for standard templates (can be absolute or env-interpolated).  
  # You can set this via process env or secrets.env:  
  #   REPORTFORGE_TEMPLATES_ROOT=C:\\Reports\\Templates  
  root: "${REPORTFORGE_TEMPLATES_ROOT}"

  # A small catalog of standard templates with metadata.  
  # Path can be absolute, relative to templates.root, or env-interpolated.  
  catalog:  
    - id: "std-monthly-ops"  
      name: "Standard Monthly Ops"  
      description: "Default monthly ops report template"  
      path: "${REPORTFORGE_TEMPLATES_ROOT}/MonthlyOps.xlsx"  
      tags: ["ops", "monthly", "standard"]

    - id: "std-weekly-kpi"  
      name: "Standard Weekly KPI"  
      description: "Weekly KPI rollup template"  
      path: "${REPORTFORGE_TEMPLATES_ROOT}/WeeklyKPI.xlsx"  
      tags: ["kpi", "weekly", "standard"]

connections:  
  # Commonly used database connection aliases (no passwords here).  
  #  
  # Recommended: keep usernames/passwords in module secrets.env (or process env),  
  # then reference them by name here.  
  #  
  # Example secrets.env entries:  
  #   OPS_SQL_SERVER=sql01.corp.local  
  #   OPS_SQL_DATABASE=OpsWarehouse  
  #   OPS_SQL_USERNAME=report_user  
  #   OPS_SQL_PASSWORD=...  
  #  
  - name: "OpsWarehouse"  
    type: "sqlserver"  
    server: "${OPS_SQL_SERVER}"  
    database: "${OPS_SQL_DATABASE}"  
    auth:  
      mode: "env"                 # integrated | env  
      username_env: "OPS_SQL_USERNAME"  
      password_env: "OPS_SQL_PASSWORD"

  - name: "FinanceDW"  
    type: "sqlserver"  
    server: "${FIN_SQL_SERVER}"  
    database: "${FIN_SQL_DATABASE}"  
    auth:  
      mode: "integrated"          # uses Windows/domain auth; no secrets needed

defaults:  
  # Optional default behaviors a report config can inherit if it omits values.  
  logging:  
    level: "info"                 # trace | debug | info | warn | error  
    ndjson: true  
  output:  
    folder_name: "output"  
    overwrite: false
```

## 5. Technical Implementation Details

### 5.1 C# DTO Contract

To ensure clean interop, C# will define the configuration structure:

- **RawConfig**: Matches YAML exactly, strings may contain ${} placeholders.  
- **ResolvedConfig**: Result of the C# Loader. All placeholders resolved, paths absolute, secrets injected into secure properties.  
- **PowerShell Responsibility**: Passes the path to the loader, receives ResolvedConfig, and passes parts of it to the Engines.

### 5.2 Build System

- **Build Script (build.ps1)**:  
  - Runs dotnet build -c Release.  
  - Copies compiled DLLs + dependencies (YamlDotNet, ClosedXML, Parquet.Net) to Output/ReportForge/Assemblies.  
  - Copies PowerShell scripts to Output/ReportForge.  
  - Updates Module Manifest RequiredAssemblies list.

## 6. Release Criteria (Definition of Done)

The project is considered V1.0 complete only when the following are satisfied:

- **Testing:**  
  - Unit tests for Config Parsing and Env Variable resolution.  
  - Integration tests for SQL -> Excel and SQL -> Parquet flows.  
  - Smoke test on a clean environment (fresh install).  
- **Documentation:**  
  - README with "3-minute Quick Start".  
  - Full Configuration Schema reference.  
  - Example folder (Config + SQL + Template).  
- **Packaging:**  
  - CI Pipeline running on GitHub Actions.  
  - Published to PowerShell Gallery.  
  - SemVer versioning applied.  
- **Security:**  
  - No secrets logged in Debug/Verbose streams.  
  - .env loading precedence verified.

## 7. Future Roadmap (v2+)

- Power BI REST API integration for dataset refresh.  
- Dataset diffing (compare current run vs. previous run).  
- Support for SecretManagement module vaults.
- Support for other types of SQL databases like POSTGRESQL or SQLite
- Support for other data sources: flat files, shared or online spreadsheet, APIs, SharePoint, Azure, AWS, et cetera.
- Additional developer tools, specifically ones to aid in the report design process, alleviating the need to manually create the report template.
- Far Future Release: Add an Orchestrator so even automation scheduling can be facilitated by this software; Add support for common, customizable dashboard libraries like Plotly and Dash in python or D3.js for Javascript.
- If surprisingly popular/well received, a future Vx.0 could also implement a distribution interface, a kind of reporting/dashboard and BI tools catalog.
  - But it wouldn't be limited to just linking to some other place the report lives
  - Could add our own custom web app capable of displaying the reports and dashes and available BI tools on its own domain.
  - Basically, this PowerShell module is just the beginning, a stepping stone, towards possibly developing a robust and open-source BI web and software platform.
