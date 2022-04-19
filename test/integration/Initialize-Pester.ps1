# https://github.com/pester/Pester/issues/1953#issuecomment-1101705143
#
# We're just looking to override an internal function to get ANSI colors in CI.
# Once the capability is available natively in Pester we'll bump our minimum
# and remove this.

$pester = Import-Module -Name Pester -MinimumVersion 5.2.0 -Force -PassThru -ErrorAction Stop

$pester.Invoke({
    $file = "$PSScriptRoot/../.." |
        Resolve-Path |
        Join-Path -ChildPath .powershell/Write-Host.ps1

    . $file

    $SafeCommands['Write-Host'] = Get-Command -Type Function -Name Write-Host
})
