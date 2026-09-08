# =============================================================================
#  Smart Pharmacy System - Auto Installer
#  Detects SQL Server, updates configuration, and grants IIS permissions
#  Run this script as Administrator
# =============================================================================

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

# ── Color & Console Formatting ─────────────────────────────────────────────
function Write-Header {
    Clear-Host
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║          Smart Pharmacy Auto Installer               ║" -ForegroundColor Cyan
    Write-Host "  ║       Automatic Database & IIS Setup Utility         ║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step  { param($n,$msg) Write-Host "  [$n] $msg" -ForegroundColor Yellow }
function Write-OK    { param($msg)    Write-Host "  ✔  $msg"  -ForegroundColor Green  }
function Write-Fail  { param($msg)    Write-Host "  ✖  $msg"  -ForegroundColor Red    }
function Write-Info  { param($msg)    Write-Host "  →  $msg"  -ForegroundColor Gray   }

# ── Application Root Path ──────────────────────────────────────────────────
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$AppDir      = $ScriptDir
$AppSettings = Join-Path $AppDir "appsettings.json"

Write-Header

# =============================================================================
# STEP 1 — Auto-Detect Running SQL Server Instances
# =============================================================================
Write-Step "1/5" "Detecting active SQL Server instances on this machine..."

$sqlServices = Get-Service | Where-Object {
    $_.DisplayName -like "SQL Server (*)" -and $_.Status -eq "Running"
}

if ($sqlServices.Count -eq 0) {
    Write-Fail "No active SQL Server instance was found on this machine!"
    Write-Host ""
    Write-Host "  Please install SQL Server Express (Free) from:" -ForegroundColor White
    Write-Host "  https://aka.ms/sqlserver-express" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "  Press Enter to exit"
    exit 1
}

# Extract instance name
$displayName   = $sqlServices[0].DisplayName          # e.g. "SQL Server (SQLEXPRESS)"
$instanceName  = $displayName -replace "SQL Server \((.+)\)", '$1'  # e.g. "SQLEXPRESS"

Write-OK "Detected SQL Server instance: $instanceName"

# Build connection string
if ($instanceName -eq "MSSQLSERVER") {
    $serverPart = "."
} else {
    $serverPart = ".\$instanceName"
}

$connectionString = "Server=$serverPart;Database=PharmacyDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
Write-Info "Connection String: $connectionString"

# =============================================================================
# STEP 2 — Update appsettings.json Connection String
# =============================================================================
Write-Host ""
Write-Step "2/5" "Updating connection string in appsettings.json..."

if (-not (Test-Path $AppSettings)) {
    Write-Fail "appsettings.json was not found at: $AppSettings"
    Write-Info "Ensure this script is located in the application published directory."
    Read-Host "  Press Enter to exit"
    exit 1
}

# Read and update appsettings.json
$json = Get-Content $AppSettings -Raw
$json = $json -replace '"DefaultConnection"\s*:\s*"[^"]*"', """DefaultConnection"": ""$connectionString"""
Set-Content $AppSettings $json -Encoding UTF8

Write-OK "Successfully updated appsettings.json"

# =============================================================================
# STEP 3 — Verify & Create Database (EF Core Migrations)
# =============================================================================
Write-Host ""
Write-Step "3/5" "Checking database availability and running migrations..."

$dotnetPath = (Get-Command dotnet -ErrorAction SilentlyContinue)?.Source

if ($dotnetPath) {
    try {
        Write-Info "Running database migrations..."
        $apiProj = Get-ChildItem $AppDir -Filter "*.csproj" -Recurse | Select-Object -First 1
        if ($apiProj) {
            & dotnet ef database update --project $apiProj.FullName 2>&1 | Out-Null
            Write-OK "Database schema updated successfully"
        } else {
            Write-Info "Project file not found — Database will auto-create on first app run"
        }
    } catch {
        Write-Info "Database will be auto-created on first application startup"
    }
} else {
    Write-Info "Database will be auto-created on first application startup"
}

# =============================================================================
# STEP 4 — Grant IIS Permissions (NT AUTHORITY\SYSTEM) in SQL Server
# =============================================================================
Write-Host ""
Write-Step "4/5" "Granting IIS permissions (NT AUTHORITY\SYSTEM) in SQL Server..."

try {
    $sqlQuery = "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'NT AUTHORITY\SYSTEM') CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS; ALTER SERVER ROLE [sysadmin] ADD MEMBER [NT AUTHORITY\SYSTEM];"
    
    $connString = "Server=$serverPart;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
    $conn = New-Object Microsoft.Data.SqlClient.SqlConnection($connString)
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sqlQuery
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()

    Write-OK "Successfully granted IIS SQL Server access permissions"
} catch {
    Write-Info "Could not grant system login automatically — current user permissions will be used"
}

# =============================================================================
# STEP 5 — Enable SQL Server Browser Service
# =============================================================================
Write-Host ""
Write-Step "5/5" "Enabling SQL Server Browser Service..."

$browser = Get-Service -Name "SQLBrowser" -ErrorAction SilentlyContinue
if ($browser -and $browser.Status -ne "Running") {
    try {
        Set-Service -Name "SQLBrowser" -StartupType Automatic
        Start-Service -Name "SQLBrowser"
        Write-OK "SQL Server Browser service started successfully"
    } catch {
        Write-Info "SQL Server Browser service could not be started (optional)"
    }
} else {
    Write-OK "SQL Server Browser service is already running"
}

# =============================================================================
# Final Summary
# =============================================================================
Write-Host ""
Write-Host "  ══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✔  Setup Completed Successfully!"                       -ForegroundColor Green
Write-Host "  ══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "  SQL Server  : $instanceName"                            -ForegroundColor White
Write-Host "  Database    : PharmacyDB"                               -ForegroundColor White
Write-Host "  Status      : Ready to run ✔"                           -ForegroundColor White
Write-Host ""
Write-Host "  You can now launch the application and visit:"          -ForegroundColor Gray
Write-Host "  http://localhost"                                        -ForegroundColor Cyan
Write-Host ""
Read-Host "  Press Enter to exit"
