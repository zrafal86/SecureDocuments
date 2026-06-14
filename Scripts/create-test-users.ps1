<#
.SYNOPSIS
    Creates one test user per role so the application can be explored without setting up real accounts.

.PARAMETER DataFolder
    Path to the folder that will hold the encrypted data files (app_config.dtms).
    The folder is created if it does not exist.

.EXAMPLE
    # Run from the repository root
    .\Scripts\create-test-users.ps1 -DataFolder "C:\SecureDocuments\test-data"
#>

param(
    [Parameter(Mandatory, HelpMessage = "Path to the folder that will store encrypted data files")]
    [string]$DataFolder
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot   = Split-Path $PSScriptRoot -Parent
$projectCli = Join-Path $repoRoot "SecureDocuments.ConsoleUtils"
$sampleJson = Join-Path $repoRoot "configFile.sample.json"
$configFile = Join-Path $DataFolder "app_config.dtms"

function Invoke-Cli {
    param([string[]]$Args)
    dotnet run --project $projectCli -- @Args
    if ($LASTEXITCODE -ne 0) {
        throw "ConsoleUtils exited with code $LASTEXITCODE"
    }
}

# ── 1. Prepare the data folder ────────────────────────────────────────────────
if (-not (Test-Path $DataFolder)) {
    New-Item -ItemType Directory -Path $DataFolder -Force | Out-Null
    Write-Host "[+] Created data folder: $DataFolder"
} else {
    Write-Host "[i] Data folder already exists: $DataFolder"
}

# ── 2. Create encrypted config from sample ────────────────────────────────────
if (Test-Path $configFile) {
    Write-Host "[i] app_config.dtms already exists — skipping encryption step."
} else {
    Write-Host "[+] Encrypting sample config -> app_config.dtms ..."
    Invoke-Cli "secure", "run", "-method", "encrypt", "-in", $sampleJson, "-out", $configFile
    Write-Host "[+] Config file created."
}

# ── 3. Reset the built-in admin password to something known ───────────────────
Write-Host "[+] Resetting built-in admin password ..."
Invoke-Cli "users", "run", "-method", "changePass",
           "-e",    "admin@admin.pl",
           "-pass", "Admin@1234",
           "-path", $configFile

# ── 4. Add one test user per role ─────────────────────────────────────────────
$testUsers = @(
    [pscustomobject]@{ Role = "0"; Email = "creator@test.local";      FirstName = "Anna";      LastName = "Kowalska";     Password = "Test@Creator1"  },
    [pscustomobject]@{ Role = "1"; Email = "admin@test.local";        FirstName = "Jan";       LastName = "Nowak";        Password = "Test@Admin1"    },
    [pscustomobject]@{ Role = "2"; Email = "manager@test.local";      FirstName = "Piotr";     LastName = "Wisniewski";   Password = "Test@Manager1"  },
    [pscustomobject]@{ Role = "3"; Email = "builder@test.local";      FirstName = "Marek";     LastName = "Kowalczyk";    Password = "Test@Builder1"  },
    [pscustomobject]@{ Role = "4"; Email = "technologist@test.local"; FirstName = "Katarzyna"; LastName = "Zajac";        Password = "Test@Tech1"     },
    [pscustomobject]@{ Role = "5"; Email = "reader@test.local";       FirstName = "Tomasz";    LastName = "Lewandowski";  Password = "Test@Reader1"   }
)

foreach ($u in $testUsers) {
    Write-Host "[+] Adding $($u.Email) (role $($u.Role)) ..."
    Invoke-Cli "users", "run", "-method", "add",
               "-e",    $u.Email,
               "-f",    $u.FirstName,
               "-l",    $u.LastName,
               "-pass", $u.Password,
               "-role", $u.Role,
               "-path", $configFile
}

# ── 5. Summary ────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "================================================================"
Write-Host "  Test users created successfully"
Write-Host "  Data folder : $DataFolder"
Write-Host "================================================================"
Write-Host ""
Write-Host "  Email                        Password          Role"
Write-Host "  ---------------------------  ----------------  -------------------------"
Write-Host "  admin@admin.pl               Admin@1234        Creator (0) - built-in"
Write-Host "  creator@test.local           Test@Creator1     Creator (0)"
Write-Host "  admin@test.local             Test@Admin1       Admin (1)"
Write-Host "  manager@test.local           Test@Manager1     Manager (2)"
Write-Host "  builder@test.local           Test@Builder1     Builder (3)"
Write-Host "  technologist@test.local      Test@Tech1        Technologist (4)"
Write-Host "  reader@test.local            Test@Reader1      Reader (5)"
Write-Host ""
Write-Host "  Start the app, select the data folder above, and log in."
Write-Host "================================================================"
