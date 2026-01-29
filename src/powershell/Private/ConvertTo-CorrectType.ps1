function ConvertTo-CorrectType {
    param (
        $value
    )

    $type = "string"

    if ($value -match "^(true|false)$") {
        $type = "boolean"
    }
    elseif ($value -match "^-?\d+(\.\d+)?$") {
        $type = "number"
    }
    elseif ([string]$value -as [datetime]) {
        $type = "datetime"
    }

    switch ($type) {
        "number" {
            $value = [double]$value
        }
        "boolean" {
            $value = [boolean]$value
        }
        "datetime" {
            $value = [datetime]$value
        }
    }

    return $value   

}