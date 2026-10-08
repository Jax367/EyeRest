param([string]$IsccPath = '', [switch]$SkipTests)
$ErrorActionPreference = 'Stop'
if (!$IsccPath) {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { $IsccPath = $command.Source }
    else {
        $candidates = @("${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:ProgramFiles\Inno Setup 7\ISCC.exe")
        $IsccPath = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    }
}
if (!$IsccPath -or !(Test-Path -LiteralPath $IsccPath)) { throw 'Install Inno Setup or specify -IsccPath.' }
$source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'src\Program.cs') -Raw
if ($source -notmatch 'AssemblyVersion\("(\d+\.\d+\.\d+)\.\d+"\)') { throw 'Assembly version not found.' }
$version = $Matches[1]
$output = Join-Path $PSScriptRoot 'build\release'
& (Join-Path $PSScriptRoot 'build.ps1') -Test:(!$SkipTests) -OutputDirectory 'build\release'
$portable = Join-Path $output "EyeRest-$version-portable.zip"
Copy-Item -LiteralPath (Join-Path $output 'EyeRest-portable.zip') -Destination $portable -Force
& $IsccPath "/DAppVersion=$version" "/DBuildDirectory=$output" (Join-Path $PSScriptRoot 'installer\EyeRest.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$installer = Join-Path $output "EyeRest-$version-Setup.exe"
$checksums = @($portable, $installer) | ForEach-Object {
    $hash = Get-FileHash -LiteralPath $_ -Algorithm SHA256
    '{0}  {1}' -f $hash.Hash.ToLowerInvariant(), [IO.Path]::GetFileName($_)
}
$checksums | Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS.txt') -Encoding ascii
Write-Output "Release $version ready: $output"
