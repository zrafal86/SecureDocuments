<#
.SYNOPSIS
    Creates one test user per role so the application can be explored without
    setting up real accounts.

.PARAMETER DataFolder
    Path to the folder that will hold the encrypted data files (app_config.dtms).
    The folder is created if it does not exist.

.EXAMPLE
    # Run from the repository root:
    .\Scripts\create-test-users.ps1 -DataFolder "C:\SecureDocuments\test-data"
#>

param(
    [Parameter(Mandatory, HelpMessage = "Folder where app_config.dtms will be created")]
    [string]$DataFolder
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot   = Split-Path $PSScriptRoot -Parent
$projectCli = Join-Path $repoRoot "SecureDocuments.ConsoleUtils" "SecureDocuments.ConsoleUtils.csproj"
$sampleJson = Join-Path $repoRoot "configFile.sample.json"
$configFile = Join-Path $DataFolder "app_config.dtms"

# ── helper ────────────────────────────────────────────────────────────────────
# Accepts an explicit [string[]] — avoids [CmdletBinding()] which causes
# PowerShell to intercept -in as -InformationAction/-InformationVariable.
# Call sites pass arguments as an @(...) array so PS never parses them.

function Invoke-Cli ([string[]] $CliArgs) {
    Write-Host "  > dotnet run ... -- $CliArgs"
    & dotnet run --project $projectCli -- @CliArgs
    if ($LASTEXITCODE -ne 0) {
        throw "ConsoleUtils failed with exit code $LASTEXITCODE"
    }
}

# ── pre-flight ────────────────────────────────────────────────────────────────

if (-not (Test-Path $projectCli)) { throw "ConsoleUtils project not found: $projectCli" }
if (-not (Test-Path $sampleJson)) { throw "Sample config not found: $sampleJson" }

Write-Host "[*] Building ConsoleUtils ..."
& dotnet build $projectCli -c Debug --nologo -v quiet
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# ── 1. Create the data folder ─────────────────────────────────────────────────

if (-not (Test-Path $DataFolder)) {
    New-Item -ItemType Directory -Path $DataFolder -Force | Out-Null
    Write-Host "[+] Created data folder: $DataFolder"
} else {
    Write-Host "[i] Data folder already exists: $DataFolder"
}

# ── 2. Encrypt the sample JSON → app_config.dtms ─────────────────────────────

if (Test-Path $configFile) {
    Write-Host "[i] app_config.dtms already exists — skipping encryption step."
} else {
    Write-Host "[+] Encrypting configFile.sample.json -> app_config.dtms ..."
    Invoke-Cli @("secure", "run", "-method", "encrypt", "-in", $sampleJson, "-out", $configFile)

    if (-not (Test-Path $configFile)) { throw "Encryption step produced no output file: $configFile" }
    Write-Host "[+] app_config.dtms created successfully."
}

# ── 3. Reset the built-in admin password to a known value ────────────────────

Write-Host "[+] Resetting built-in admin@admin.pl password ..."
Invoke-Cli @("users", "run", "-method", "changePass", "-e", "admin@admin.pl", "-pass", "Admin@1234", "-path", $configFile)

# ── 4. Add one test user per role ─────────────────────────────────────────────

$testUsers = @(
    @{ Role = "0"; Email = "creator@test.local";      First = "Anna";      Last = "Kowalska";    Pass = "Test@Creator1" }
    @{ Role = "1"; Email = "admin@test.local";        First = "Jan";       Last = "Nowak";       Pass = "Test@Admin1"   }
    @{ Role = "2"; Email = "manager@test.local";      First = "Piotr";     Last = "Wisniewski";  Pass = "Test@Manager1" }
    @{ Role = "3"; Email = "builder@test.local";      First = "Marek";     Last = "Kowalczyk";   Pass = "Test@Builder1" }
    @{ Role = "4"; Email = "technologist@test.local"; First = "Katarzyna"; Last = "Zajac";       Pass = "Test@Tech1"    }
    @{ Role = "5"; Email = "reader@test.local";       First = "Tomasz";    Last = "Lewandowski"; Pass = "Test@Reader1"  }
)

foreach ($u in $testUsers) {
    Write-Host "[+] Adding $($u.Email)  (role $($u.Role)) ..."
    Invoke-Cli @("users", "run", "-method", "add",
        "-e",    $u.Email,
        "-f",    $u.First,
        "-l",    $u.Last,
        "-pass", $u.Pass,
        "-role", $u.Role,
        "-path", $configFile)
}

# ── 5. Summary ────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "================================================================"
Write-Host "  Done!  Data folder: $DataFolder"
Write-Host "================================================================"
Write-Host ""
Write-Host "  Email                          Password           Role"
Write-Host "  -----------------------------  -----------------  ----------------------"
Write-Host "  admin@admin.pl                 Admin@1234         Creator (0) built-in"
Write-Host "  creator@test.local             Test@Creator1      Creator (0)"
Write-Host "  admin@test.local               Test@Admin1        Admin (1)"
Write-Host "  manager@test.local             Test@Manager1      Manager (2)"
Write-Host "  builder@test.local             Test@Builder1      Builder (3)"
Write-Host "  technologist@test.local        Test@Tech1         Technologist (4)"
Write-Host "  reader@test.local              Test@Reader1       Reader (5)"
Write-Host ""
Write-Host "  Next step: start the app and select '$DataFolder' as the data folder."
Write-Host "================================================================"
