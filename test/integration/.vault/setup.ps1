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

& $VaultPath server -dev &

& $VaultPath secrets enable -path kv -version 1 kv
& $VaultPath kv put kv/win/gmsa-getter Username=$CcgUser Password=$CcgPass Domain=$CcgDomain
& $VaultPath kv put secret/win/gmsa-getter Username=$CcgUser Password=$CcgPass Domain=$CcgDomain
