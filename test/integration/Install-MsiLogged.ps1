# Inspired by https://serverfault.com/a/1007421/236470
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [Alias('Path')]
    [ValidateScript({
        $_.Exists
    })]
    [System.IO.FileInfo]
    $LiteralPath,

    [Parameter(Mandatory)]
    [ValidateScript({
        -not [System.IO.DirectoryInfo]::new($_.FullName).Exists
    })]
    [System.IO.FileInfo]
    $LogFile,

    [Parameter()]
    [int]
    $TimeoutMs = (5 * 60 * 1000)
    #             ^ minutes
)

End {
    $main_process = Start-Process -FilePath "msiexec.exe" -ArgumentList @(
        '/I'
        $LiteralPath.FullName
        '/QN'
        '/L*v!'
        $LogFile.FullName
    ) -NoNewWindow -PassThru

    $log_process = Start-Process -FilePath ([System.Diagnostics.Process]::GetCurrentProcess().Path) -ArgumentList @(
        '-c'
        "Get-Content -LiteralPath $($LogFile.FullName)  -Wait"
    ) -NoNewWindow -PassThru

    $exited = $main_process.WaitForExit($TimeoutMs)
    $log_process.Kill()

    if ($exited) {
        exit $main_process.ExitCode
    }
    else {
        # timed out
        Write-Host -Object "Timed out after $TimeoutMs ms waiting for process to exit."
        $main_process.Kill($true)
        exit -1
    }
}
