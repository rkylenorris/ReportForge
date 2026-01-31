function Import-EnvVariables {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [string]
        $envPath = ".\.env"
    )

    process {
        # check .env file exists
        if (Test-Path $envPath) {
            # define regex pattern for finding keys and values
            $pattern = "^\s*([^=]+)\s*=\s*(.+)\s*$"
            $keyPattern = "^[A-Z_][A-Z0-9_]*$"
            # import the .env contents           
            $envContent = Get-Content $envPath -Raw
            if ($envContent) {
                foreach ($line in ($envContent -split "`n")) {
                    # skip line if blank or commented out
                    if ($line.StartsWith('#') -or [string]::IsNullOrWhiteSpace($line)) {
                        continue
                    }

                    # match against regex to get keys and values 
                    if ($line -match $pattern) {
                        # when match is found, set env variable       
                        $varName = $matches[1]
                        $varValue = $matches[2]
                        $varValue = ConvertTo-CorrectType -value $varValue # convert from string to correct type       
                        [System.Environment]::SetEnvironmentVariable($varName, $varValue) # env var name, env var value
                    }
                    else {
                        Write-Warning "Line $line not formatted correctly for import, should be name=value"
                    }
                }
            }
            else {
                Write-Warning "No content found in .env file"
            }
        }
        else {
            throw [System.IO.FileNotFoundException] "env file $envPath does not exist"
        }
    }
}