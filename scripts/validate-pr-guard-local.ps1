Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Set-Location (Join-Path $PSScriptRoot '..')

function Invoke-GuardCheck {
    param(
        [string]$Title,
        [string]$Body,
        [string[]]$RequiredPatterns
    )

    $isAi = $false
    if ($Title -match '(?i)\[ai-assisted\]|\[ai\]') {
        $isAi = $true
    }

    if (-not $isAi) {
        return [pscustomobject]@{ Passed = $true; Message = 'Non AI-assisted PR: guard skipped.' }
    }

    foreach ($pattern in $RequiredPatterns) {
        if ($Body -notmatch [regex]::Escape($pattern)) {
            return [pscustomobject]@{ Passed = $false; Message = "Missing required AI verification field: $pattern" }
        }
    }

    if ($Body -match 'xx\.xx%|pass/fail') {
        return [pscustomobject]@{ Passed = $false; Message = 'AI verification fields still have placeholders.' }
    }

    return [pscustomobject]@{ Passed = $true; Message = 'AI-assisted PR body validation passed.' }
}

$workflow = Get-Content '.github/workflows/pr-review-agent.yml' -Raw
$template = Get-Content '.github/pull_request_template.md' -Raw

$quoted = [regex]::Matches($workflow, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
$requiredPatterns = $quoted | Where-Object { $_ -like '## *' -or $_ -like '- *:' }

$aiBlock = [regex]::Match($template, '(?s)```text\s*(.*?)\s*```').Groups[1].Value

$successBody = $aiBlock
$successBody = $successBody -replace 'pass/fail', 'pass'
$successBody = $successBody -replace '\(passed/failed/skipped\)', '(59/59, failed 0, skipped 0)'
$successBody = $successBody -replace 'line xx\.xx%, branch xx\.xx%', 'line 70.35%, branch 54.23%'

$failureBody = $aiBlock

$success = Invoke-GuardCheck -Title '[AI] success case' -Body $successBody -RequiredPatterns $requiredPatterns
$failure = Invoke-GuardCheck -Title '[AI] failure case' -Body $failureBody -RequiredPatterns $requiredPatterns

Write-Output "SUCCESS => Passed=$($success.Passed); Message=$($success.Message)"
Write-Output "FAILURE => Passed=$($failure.Passed); Message=$($failure.Message)"
