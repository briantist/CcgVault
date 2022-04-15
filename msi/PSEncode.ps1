[CmdletBinding()]
[OutputType([string])]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]
    $Path
)

End {
    $command = Get-Content -Path $Path -Raw
    $bytes = [System.Text.Encoding]::Unicode.GetBytes($command)
    [Convert]::ToBase64String($bytes)
}
