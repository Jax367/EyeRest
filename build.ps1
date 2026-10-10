param([switch]$Test, [string]$OutputDirectory = 'build')
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Please install .NET Framework 4.8.' }
$output = Join-Path $PSScriptRoot $OutputDirectory
New-Item -ItemType Directory -Force -Path $output | Out-Null
$sources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | ForEach-Object { $_.FullName })
& $compiler /nologo /target:winexe /optimize+ /platform:anycpu /codepage:65001 /warn:4 /warnaserror+ /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Runtime.Serialization.dll "/resource:$PSScriptRoot\assets\netzach-user-chibi-transparent.png,EyeRest.user-chibi.png" "/resource:$PSScriptRoot\assets\netzach-user-standing.png,EyeRest.user-standing.png" "/resource:$PSScriptRoot\assets\netzach-weary.png,EyeRest.weary.png" "/win32manifest:$PSScriptRoot\app.manifest" "/out:$output\EyeRest.exe" $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'EyeRest.exe.config') -Destination $output
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $output
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.en.md') -Destination $output
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.ja.md') -Destination $output
if ($Test) {
    $report = Join-Path $output 'test-results.txt'
    $process = Start-Process -FilePath "$output\EyeRest.exe" -ArgumentList '--self-test', "`"$report`"" -Wait -PassThru -WindowStyle Hidden
    if (Test-Path -LiteralPath $report) { Get-Content -LiteralPath $report -Encoding UTF8 }
    if ($process.ExitCode -ne 0) { throw 'Tests failed.' }
}
Compress-Archive -LiteralPath "$output\EyeRest.exe", "$output\EyeRest.exe.config", "$output\README.md", "$output\README.en.md", "$output\README.ja.md" -DestinationPath "$output\EyeRest-portable.zip" -Force
Write-Output "Built: $output\EyeRest.exe"
