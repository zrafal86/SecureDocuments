# SecureDocuments

A secure document management desktop application for shipyard operations. All data is stored as AES-256 encrypted files on disk — no database required.

## Features

- Role-based access control (6 roles: Creator → Reader)
- AES-256 encrypted data files (`.dtms`) — nothing stored in plaintext
- Manage shipyard offers: create, track status, attach files
- Localization support: Polish (default) and English
- Session persistence across restarts
- Console utility for user and data administration

---

## Technology Stack

| Layer | Technology |
|---|---|
| UI | WPF (.NET 10, Windows only) |
| UI framework | ReactiveUI 18 + Material Design |
| Encryption | AES-256-CBC + PBKDF2 (20 000 rounds) |
| Serialization | Newtonsoft.Json |
| Logging | Serilog (file + console) |
| Admin CLI | ConsoleAppFramework |

---

## Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| Windows | 10 or later | WPF requires Windows |
| .NET SDK | 10.0 | [Download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Git | any | to clone the repository |

Verify your .NET installation:

```powershell
dotnet --version
# expected: 10.x.x
```

---

## Quick Start with Test Users

The fastest way to explore all roles is the included PowerShell script. Run it once from the repository root — it creates an encrypted config and adds one user per role with known passwords:

```powershell
.\Scripts\create-test-users.ps1 -DataFolder "C:\SecureDocuments\test-data"
```

After it finishes, start the application (`dotnet run --project SecureDocuments.WPF`), select `C:\SecureDocuments\test-data` as the data folder, and log in with any of the credentials printed by the script.

See [Test Users](#test-users) for the full credentials table.

---

## Getting Started

Follow these steps in order. You need to create an encrypted configuration file before the application can be used for the first time.

### Step 1 — Clone and build

```powershell
git clone https://github.com/zrafa/SecureDocuments.git
cd SecureDocuments
dotnet build SecureDocuments.sln
```

A successful build produces no errors. Warnings are expected and can be ignored.

### Step 2 — Create a data folder

The application stores all encrypted data in a folder you choose. Create it now:

```powershell
mkdir C:\SecureDocuments\data
```

You can place this folder anywhere. Remember the path — you will need it every time you log in.

### Step 3 — Create the initial configuration file

The application reads user accounts from an encrypted file called `app_config.dtms`. You create it by encrypting the provided sample JSON:

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- secure run `
  -method encrypt `
  -in configFile.sample.json `
  -out C:\SecureDocuments\data\app_config.dtms
```

This writes the encrypted configuration file to your data folder. The sample config includes one admin account (`admin@admin.pl`) with a placeholder password hash.

### Step 4 — Set a known password for the admin account

The password stored in `configFile.sample.json` is a hash of an unknown value, so you cannot log in with it directly. Replace it with a password you choose:

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- users run `
  -method changePass `
  -e admin@admin.pl `
  -pass YourPassword123 `
  -path C:\SecureDocuments\data\app_config.dtms
```

Replace `YourPassword123` with any password you want to use.

### Step 5 — Run the application

```powershell
dotnet run --project SecureDocuments.WPF
```

Or open `SecureDocuments.sln` in Visual Studio and press **F5**.

### Step 6 — Log in

On the login screen:

1. Click **Browse** (or the folder icon) and select the data folder you created in Step 2 (`C:\SecureDocuments\data`).
2. Enter the email: `admin@admin.pl`
3. Enter the password you set in Step 4.
4. Click **Login**.

The application remembers the folder between sessions — you only need to browse once.

---

## User Roles

Roles form a strict hierarchy: a lower number means broader access. A user with role X has access to all features that require role X **or any higher-numbered role**.

### Permission matrix

| Feature | Creator (0) | Admin (1) | Manager (2) | Builder (3) | Technologist (4) | Reader (5) |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| View offers list | Yes | Yes | Yes | Yes | Yes | Yes |
| Create a new offer | Yes | Yes | Yes | Yes | Yes | Yes |
| Edit & save offer details | Yes | Yes | Yes | No | No | No |
| Change offer status (Accept / Reject / Finish / Archive) | Yes | Yes | Yes | No | No | No |
| Export offers to CSV | Yes | Yes | No | No | No | No |
| Manage files attached to an offer | Yes | Yes | Yes | No | No | No |

### Role descriptions

| Role | Value | What it represents |
|---|---|---|
| Creator | 0 | Application owner / super-admin. Full unrestricted access. |
| Admin | 1 | Senior user. Can export data to CSV and appears as the **Applicant** when assigning staff to an offer. |
| Manager | 2 | Day-to-day operator. Can edit offer details, change statuses, and appears as the **Manager** assignee. |
| Builder | 3 | Construction specialist. Read-only in the UI; appears in the **Builder** assignee dropdown on an offer. |
| Technologist | 4 | Technical specialist. Read-only in the UI; appears in the **Technologist** assignee dropdown on an offer. |
| Reader | 5 | Audit / reporting access. Can only view offers — no write actions allowed. |

> **How access is evaluated** — `RoleAccessService` builds an access list of all role values >= the user's own role value. A feature gate like `CheckPermission(Role.Manager)` passes only if the Manager role value (2) is in that list, meaning the user must be Manager, Admin, or Creator.

---

## Test Users

Use the script below to create one account per role in a local data folder. All credentials are test-only and should not be used in production.

```powershell
# Run from the repository root
.\Scripts\create-test-users.ps1 -DataFolder "C:\SecureDocuments\test-data"
```

The script will:
1. Create the data folder if it does not exist.
2. Encrypt `configFile.sample.json` into `app_config.dtms`.
3. Reset the built-in `admin@admin.pl` password to `Admin@1234`.
4. Add six users — one per role — with the passwords listed below.

### Credentials

| Email | Password | Role |
|---|---|---|
| `admin@admin.pl` | `Admin@1234` | Creator (0) — built-in sample account |
| `creator@test.local` | `Test@Creator1` | Creator (0) |
| `admin@test.local` | `Test@Admin1` | Admin (1) |
| `manager@test.local` | `Test@Manager1` | Manager (2) |
| `builder@test.local` | `Test@Builder1` | Builder (3) |
| `technologist@test.local` | `Test@Tech1` | Technologist (4) |
| `reader@test.local` | `Test@Reader1` | Reader (5) |

Log in with a Manager or higher role first to create some offers, then switch to Builder, Technologist, or Reader to observe the read-only experience.

---

## Managing Users (Console Utility)

All user management is done through `SecureDocuments.ConsoleUtils`. Run all commands from the repository root.

### Add a user

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- users run `
  -method add `
  -e user@example.com `
  -f John `
  -l Doe `
  -pass SecretPassword `
  -role 5 `
  -path C:\SecureDocuments\data\app_config.dtms
```

Role values: `0` = Creator, `1` = Admin, `2` = Manager, `3` = Builder, `4` = Technologist, `5` = Reader.

### Change a user's password

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- users run `
  -method changePass `
  -e user@example.com `
  -pass NewPassword `
  -path C:\SecureDocuments\data\app_config.dtms
```

### Remove a user

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- users run `
  -method del `
  -e user@example.com `
  -path C:\SecureDocuments\data\app_config.dtms
```

---

## Encrypting and Decrypting Data Files (Advanced)

The `secure` command lets you encrypt any JSON into a `.dtms` file, or decrypt a `.dtms` file back to JSON for inspection.

### Encrypt a JSON file

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- secure run `
  -method encrypt `
  -in input.json `
  -out output.dtms
```

### Decrypt a `.dtms` file

```powershell
dotnet run --project SecureDocuments.ConsoleUtils -- secure run `
  -method decrypt `
  -in output.dtms `
  -out decrypted.json
```

The `-role` flag (0–5) selects the encryption key. Default is `0` (Creator). Use the same role value for encryption and decryption.

---

## Data Files Reference

| File | Purpose |
|---|---|
| `app_config.dtms` | Encrypted user accounts, email settings, company list, country list |
| `offer.dtms` | Encrypted offers index |
| `user_session.dtms` | Encrypted current session (auto-login state) |
| `*.fdtms` | Individual offer file |
| `*.details` | Offer file metadata |

All files live in the data folder you selected at login. You can back them up by copying the entire folder.

---

## Publishing a Release Build

```powershell
# Single-file executable for Windows x64
dotnet publish SecureDocuments.WPF -c Release -r win-x64 `
  -p:PublishSingleFile=true --self-contained true

# Self-contained without single-file (separate runtime files)
dotnet publish SecureDocuments.WPF -c Release -r win-x64 --self-contained true
```

The output lands in `SecureDocuments.WPF\bin\Release\net10.0-windows\win-x64\publish\`.

---

## Running Tests

```powershell
dotnet test SecureDocuments.UnitTests
```

---

## Project Structure

```
SecureDocuments/
├── SecureDocuments.WPF/          # Desktop application (startup project)
│   ├── Views/                    # XAML screens (Login, Offers, Profile, …)
│   ├── Services/                 # WPF-specific services
│   └── Resources/                # Icons, localization (en, pl-PL)
├── SecureDocuments/              # Core library — shared by WPF and CLI
│   ├── Models/                   # User, Offer, Session, AppConfig, …
│   ├── ViewModels/               # ReactiveUI view models
│   ├── Services/                 # Business logic
│   ├── Data/                     # Encrypted file data sources
│   └── Encryption/               # AES-256 + PBKDF2 implementations
├── SecureDocuments.ConsoleUtils/ # CLI for user and file administration
├── SecureDocuments.UnitTests/    # Unit tests
├── Scripts/                      # PowerShell helper scripts
└── configFile.sample.json        # Sample configuration template
```

---

## Logs

Application logs are written to `%APPDATA%\dmfs-*.log`. Open that folder with:

```powershell
explorer $env:APPDATA
```

---

## Troubleshooting

**"Could not find config file" or blank screen after selecting folder**
The selected folder must contain `app_config.dtms`. Complete Step 3 of the Getting Started guide.

**Login fails with correct credentials**
The password hash in the config does not match. Use the `changePass` command (Step 4) to reset it to a known value.

**Build fails with SDK version error**
`global.json` pins the SDK. Install .NET SDK 10.0 or remove/update `global.json` to match your installed version.

**Application does not start (PlatformNotSupportedException)**
WPF requires Windows. The application cannot run on Linux or macOS.
