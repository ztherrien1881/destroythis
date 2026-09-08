param(
    [string]$RimWorldPath = ""
)

$ErrorActionPreference = "Stop"
$ModRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

if ([string]::IsNullOrWhiteSpace($RimWorldPath)) {
    $Candidates = @(
        "${env:ProgramFiles(x86)}\Steam\steamapps\common\RimWorld",
        "$env:ProgramFiles\Steam\steamapps\common\RimWorld",
        "C:\SteamLibrary\steamapps\common\RimWorld",
        "D:\SteamLibrary\steamapps\common\RimWorld"
    )
    $RimWorldPath = $Candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($RimWorldPath)) {
    $RimWorldPath = Read-Host "Paste your RimWorld installation folder"
}

$Managed = Join-Path $RimWorldPath "RimWorldWin64_Data\Managed"
$AssemblyCSharp = Join-Path $Managed "Assembly-CSharp.dll"
$UnityCore = Join-Path $Managed "UnityEngine.CoreModule.dll"
$NetStandard = Join-Path $Managed "netstandard.dll"
$UnityMathematics = Join-Path $Managed "Unity.Mathematics.dll"
$SteamApps = Split-Path -Parent (Split-Path -Parent $RimWorldPath)
$HarmonyRoot = Join-Path $SteamApps "workshop\content\294100\2009463077"
$HarmonyDll = Get-ChildItem -Path $HarmonyRoot -Filter "0Harmony.dll" -Recurse -ErrorAction SilentlyContinue |
    Select-Object -First 1 -ExpandProperty FullName

if (!(Test-Path $AssemblyCSharp) -or !(Test-Path $UnityCore) -or
    !(Test-Path $NetStandard) -or !(Test-Path $UnityMathematics)) {
    throw "Could not find RimWorld's managed DLLs under: $Managed"
}

if ([string]::IsNullOrWhiteSpace($HarmonyDll)) {
    $HarmonyDll = Read-Host "Could not locate Harmony automatically. Paste the full path to 0Harmony.dll"
}
if (!(Test-Path $HarmonyDll)) {
    throw "Could not find 0Harmony.dll. Make sure Harmony is subscribed and installed."
}

$CompilerCandidates = @(
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
$Compiler = $CompilerCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($Compiler)) {
    throw "Windows C# compiler was not found. Install .NET Framework 4.8 Developer Pack and run this script again."
}

$SourceDir = Join-Path $ModRoot "Source\DestroyThis"
$OutputDir = Join-Path $ModRoot "1.6\Assemblies"
$OutputDll = Join-Path $OutputDir "DestroyThis.dll"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$Sources = Get-ChildItem -Path $SourceDir -Filter "*.cs" | ForEach-Object { $_.FullName }

Write-Host "Building Destroy This..."
& $Compiler /nologo /target:library /optimize+ "/out:$OutputDll" `
    "/reference:$AssemblyCSharp" `
    "/reference:$UnityCore" `
    "/reference:$NetStandard" `
    "/reference:$UnityMathematics" `
    "/reference:$HarmonyDll" `
    $Sources
if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed. Take a screenshot or copy the red error text and send it back to me."
}

Write-Host ""
Write-Host "Build succeeded:" -ForegroundColor Green
Write-Host $OutputDll
Write-Host ""
Write-Host "Copy the entire DestroyThis folder into RimWorld\Mods to test it."
Read-Host "Press Enter to close"
