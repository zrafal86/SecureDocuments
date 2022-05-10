param(
    [Parameter(Mandatory)]
    [string]$Method,
    [Parameter(Mandatory)]
    [string]$filePath
)

$currentLocation = Get-Location
$scriptLocation = $currentLocation.Path

$encryptedFile = "$filePath\app_config.dtms"
$decryptedFile = "$filePath\app_config_decrypted.json"

if ($Method) {
    Write-Host "method: $Method"
    if ($Method -ieq "decrypt") {
        Invoke-Expression "$scriptLocation\SecureDocuments.ConsoleUtils.exe secure run -method decrypt -in $encryptedFile -out $decryptedFile"
    }
    if ($Method -ieq "encrypt") {
        Invoke-Expression "$scriptLocation\SecureDocuments.ConsoleUtils.exe secure run -method encrypt -in $decryptedFile -out $encryptedFile"
    }
}