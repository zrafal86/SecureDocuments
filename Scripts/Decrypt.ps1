param(
    $Folder
)

$currentLocation = Get-Location
$scriptLocation = $currentLocation.Path
Clear-Host
$DecryptedFolder = "Decrypted"
# .\Decrypt.ps1 -InputFile "samplefile.pdf.fdtms.details"
# DecryptFile -InputFile $InputFile

function DecryptFile {
    param(
        [Parameter(Mandatory)]
        [string]$InputFile,
        [string]$OutputFile
    )
    $OutputFile = $InputFile +"1"

    $InputAsParm = $InputFile -replace " ", "` "
    $InputAsParm = $InputAsParm -replace "`'", "`'"
    $InputAsParm = "`"$InputAsParm`""
    $OutputAsParam = $OutputFile -replace " ", "` "
    $OutputAsParam = $OutputAsParam -replace "'", "`'"
    $OutputAsParam = "`"$OutputAsParam`""
    
    Invoke-Expression "$scriptLocation\SecureDocuments.ConsoleUtils.exe secure run -method decrypt -in $InputAsParm -out $OutputAsParam"

    $jsonDetails = Get-Content $OutputFile | Out-String | ConvertFrom-Json
 
    Remove-Item $OutputFile

    $role = $jsonDetails.role
    $nameEx = $jsonDetails.nameExt
    $finalOutputFile = "`"./$DecryptedFolder/$nameEx`""
    $encryptedFileNameTemp = "$nameEx" + ".fdtms"
    $encryptedFileName = "`"$encryptedFileNameTemp`""

    Invoke-Expression "$scriptLocation\SecureDocuments.ConsoleUtils.exe secure run -method decrypt -in $encryptedFileName -out $finalOutputFile -role $role"
}

Push-Location $Folder

$exists = Test-Path -Path $DecryptedFolder
if(-not $exists){
    mkdir $DecryptedFolder
}

Get-ChildItem | Where-Object { $_.Extension -eq ".details"} | ForEach-Object { 
    $fileName = $_.Name
    Write-Host $fileName
    DecryptFile -InputFile $fileName
}

Pop-Location