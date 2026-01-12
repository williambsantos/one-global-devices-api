#!/usr/bin/env pwsh

# Configuration
$testResultsDir = "temp/00-TestResults"
$coverageReportDir = "temp/01-CoverageReport"

# Clean previous results
Remove-Item -Path $testResultsDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path $coverageReportDir -Recurse -Force -ErrorAction SilentlyContinue

# Install ReportGenerator if not available
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Run tests with coverage
Write-Host "`n=== Running Tests ===" -ForegroundColor Cyan
dotnet test --collect:"XPlat Code Coverage" --results-directory:$testResultsDir

# Find coverage file
$coverageFile = Get-ChildItem -Path $testResultsDir -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1

# Generate HTML report
Write-Host "`n=== Generating Coverage Report ===" -ForegroundColor Cyan
reportgenerator -reports:"$($coverageFile.FullName)" -targetdir:$coverageReportDir -reporttypes:"Html;JsonSummary"

# Display coverage summary
Write-Host "`n=== Coverage Summary ===" -ForegroundColor Cyan
$summary = Get-Content "$coverageReportDir\Summary.json" | ConvertFrom-Json
$lineCoverage = [math]::Round($summary.summary.linecoverage, 2)
$branchCoverage = [math]::Round($summary.summary.branchcoverage, 2)

Write-Host "Line Coverage:   $lineCoverage%" -ForegroundColor Green
Write-Host "Branch Coverage: $branchCoverage%" -ForegroundColor Green
Write-Host ""

# Open HTML report
Start-Process "$coverageReportDir\index.html"
