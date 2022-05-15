#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $VaultPath = 'vault.exe',

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $CcgUser = 'ccg-reader' ,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $CcgPass = 'password1' ,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $CcgDomain = 'contoso.com'
)

if (-not $env:VAULT_ADDR) {
    $env:VAULT_ADDR = 'http://127.0.0.1:8200'
}

if (-not $env:VAULT_DEV_ROOT_TOKEN_ID) {
    $env:VAULT_DEV_ROOT_TOKEN_ID = '323e3e66-e5fe-42ba-a4b9-fad293077754'
}

if (-not $env:VAULT_TOKEN) {
    $env:VAULT_TOKEN = $env:VAULT_DEV_ROOT_TOKEN_ID
}

$env:VAULT_CLIENT_TIMEOUT = 1
& $VaultPath status
$running = $?
$env:VAULT_CLIENT_TIMEOUT = ''

if ($running) {
    return
}

# & $VaultPath server -dev &
$w32p = Get-CimClass -ClassName Win32_Process
$w32startup = Get-CimClass -ClassName Win32_ProcessStartup
$vars = [System.Environment]::GetEnvironmentVariables([System.EnvironmentVariableTarget]::Process).GetEnumerator().ForEach({
    "{0}={1}" -f $_.Name, $_.Value
}) -as [string[]]
$vars | Write-Verbose
$startup = New-CimInstance -CiMClass $w32startup -Property @{ EnvironmentVariables = $vars } -ClientOnly
$proc = Invoke-CimMethod -CiMClass $w32p -Name Create -Arguments @{
    CommandLine = "$VaultPath server -dev"
    ProcessStartupInformation = $startup
}
$proc.ProcessId | Set-Content -LiteralPath $env:TMP\vaultpid -NoNewline -Encoding utf8NoBOM

& $VaultPath secrets enable -path kv -version 1 kv
& $VaultPath kv put kv/win/gmsa-getter Username=$CcgUser Password=$CcgPass Domain=$CcgDomain
& $VaultPath kv put secret/win/gmsa-getter Username=$CcgUser Password=$CcgPass Domain=$CcgDomain
