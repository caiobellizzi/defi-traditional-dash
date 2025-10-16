# DeFi Dashboard - Test Runner Script (PowerShell)
# This script runs all tests (unit + integration) and generates coverage reports

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Yellow
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  DeFi Dashboard - Test Runner" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Info "Using .NET version: $dotnetVersion"
} catch {
    Write-Error ".NET SDK not found. Please install .NET 9 SDK."
    exit 1
}

Write-Host ""

# Navigate to project root
$ProjectRoot = $PSScriptRoot
Set-Location $ProjectRoot

# ================================================
# 1. Run Backend Unit Tests
# ================================================
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Running Backend Unit Tests" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Set-Location "$ProjectRoot\tests\ApiService.Tests"

Write-Info "Running unit tests with coverage..."
dotnet test `
    --configuration Release `
    --logger "console;verbosity=detailed" `
    --collect:"XPlat Code Coverage" `
    --results-directory:"$ProjectRoot\test-results\unit"

if ($LASTEXITCODE -eq 0) {
    Write-Success "Unit tests passed!"
} else {
    Write-Error "Unit tests failed!"
    exit 1
}

Write-Host ""

# ================================================
# 2. Run Backend Integration Tests
# ================================================
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Running Backend Integration Tests" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Set-Location "$ProjectRoot\tests\ApiService.IntegrationTests"

Write-Info "Starting Testcontainers for PostgreSQL..."
Write-Info "Running integration tests with coverage..."
dotnet test `
    --configuration Release `
    --logger "console;verbosity=detailed" `
    --collect:"XPlat Code Coverage" `
    --results-directory:"$ProjectRoot\test-results\integration"

if ($LASTEXITCODE -eq 0) {
    Write-Success "Integration tests passed!"
} else {
    Write-Error "Integration tests failed!"
    exit 1
}

Write-Host ""

# ================================================
# 3. Generate Coverage Report
# ================================================
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Generating Coverage Report" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if reportgenerator is installed
try {
    reportgenerator -version | Out-Null
} catch {
    Write-Info "Installing ReportGenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

Write-Info "Generating HTML coverage report..."
reportgenerator `
    -reports:"$ProjectRoot\test-results\**\coverage.cobertura.xml" `
    -targetdir:"$ProjectRoot\test-results\coverage-report" `
    -reporttypes:"Html;TextSummary"

if ($LASTEXITCODE -eq 0) {
    Write-Success "Coverage report generated!"
    Write-Host ""
    Write-Info "View coverage report at: $ProjectRoot\test-results\coverage-report\index.html"
} else {
    Write-Error "Failed to generate coverage report!"
}

Write-Host ""

# ================================================
# 4. Display Coverage Summary
# ================================================
$summaryFile = "$ProjectRoot\test-results\coverage-report\Summary.txt"
if (Test-Path $summaryFile) {
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Coverage Summary" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
    Get-Content $summaryFile
    Write-Host ""
}

# ================================================
# 5. Run Frontend Tests (Optional)
# ================================================
if (Test-Path "$ProjectRoot\frontend") {
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "  Running Frontend Tests" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""

    Set-Location "$ProjectRoot\frontend"

    # Check if Node.js is installed
    try {
        npm --version | Out-Null

        Write-Info "Installing frontend dependencies..."
        npm install --silent

        # Run E2E tests if playwright is configured
        if (Test-Path "playwright.config.ts") {
            Write-Info "Running Playwright E2E tests..."
            npm run test:e2e

            if ($LASTEXITCODE -eq 0) {
                Write-Success "Frontend E2E tests passed!"
            } else {
                Write-Error "Frontend E2E tests failed!"
            }
        } else {
            Write-Info "Playwright not configured. Skipping E2E tests."
        }
    } catch {
        Write-Error "Node.js/npm not found. Skipping frontend tests."
    }

    Write-Host ""
}

# ================================================
# Summary
# ================================================
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Success "All tests completed successfully!"
Write-Host ""
Write-Info "Test Results Location: $ProjectRoot\test-results\"
Write-Info "Coverage Report: $ProjectRoot\test-results\coverage-report\index.html"
Write-Host ""

# Open coverage report in browser (optional)
$response = Read-Host "Open coverage report in browser? (y/n)"
if ($response -match "^[Yy]$") {
    Start-Process "$ProjectRoot\test-results\coverage-report\index.html"
}

Write-Host "================================================" -ForegroundColor Cyan
exit 0
