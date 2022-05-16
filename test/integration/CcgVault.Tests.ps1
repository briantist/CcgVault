#Requires -Module @{ ModuleName = 'Pester'; ModuleVersion = '5.2.0' }
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingPlainTextForPassword', 'CredSpecPath')]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $CcgVault = "$env:CcgVaultBin\CcgVault.exe" ,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $Config = 'C:\ccg\ccgvault.yml' ,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $ContainerImage = $env:CcgVaultTestContainer,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $CredSpecPath = "$env:ProgramData\Docker\credentialspecs"
)

BeforeAll {
    $data = Import-PowerShellDataFile -LiteralPath $PSScriptRoot\TestData.psd1

    $Script:ccgPluginId = [Guid]::Parse($data.ComPlus.CcgPlugin.ID)

    $kv1data = $data.CcgVault.Kv1Data
    $kv2data = $data.CcgVault.Kv2Data

    $Script:expected_data = @{
        static_kv1 = $kv1data
        static_kv2 = $kv2data
    }

    # this doesn't work for some reason, haven't figured out why yet.
    # if the path is wrong, the tests fail, so it seems like it tries to execute it at least,
    # but it does not appear to actually start the Vault server; the tests only work if I run
    # it separately first (and even in that case, tests fail when the path is wrong).
    # & "$PSScriptRoot\.vault\setup.ps1" -Kv1Data $kv1data -Kv2Data $kv2data -ErrorAction Stop -Verbose
}

Describe 'CcgVault tests' -Tag CcgVault {
    Context 'CLI test command w/ source: <source>' -Tag CLI  -Foreach @(
        @{ source = 'static_kv1' }
        @{ source = 'static_kv2' }
    ) {
        BeforeAll {
            $Script:expected = $expected_data[$source]
            $Script:output = & $CcgVault test --input "${Config}|${source}"
            $Script:result = $output |
                ForEach-Object -Begin { $Script:r = @{} } -Process {
                    $key, $value = $_ -split ': '
                    $r[$key] = $value
                } -End { $r }
        }

        It "Source '<source>' has Username '<expected.Username>'" {
            $result.User | Should -BeExactly $expected.Username
        }

        It "Source '<source>' has Domain '<expected.Domain>'" {
            $result.Domain | Should -BeExactly $expected.Domain
        }

        It "Source '<source>' has Password '<expected.Password>'" {
            $result.Pass | Should -BeExactly $expected.Password
        }
    }

    Context 'Docker run test w/ source: <source>' -Tag Docker -Foreach @(
        @{ source = 'static_kv1' }
        @{ source = 'static_kv2' }
    ) {
        BeforeAll {
            $Script:timeout = 60
            $Script:time = Get-Date
            $Script:logname = $data.CcgVault.LogName

            $credspecfile = "credspec_${source}.json"
            $credspec = Join-Path -Path $CredSpecPath -ChildPath $credspecfile

            $spec = Get-Content -LiteralPath "$PSScriptRoot/.config/credspec.json" -Raw | ConvertFrom-Json -AsHashtable -Depth 10
            $spec.ActiveDirectoryConfig.HostAccountConfig.PluginInput = "${Config}|${source}"

            ConvertTo-Json -InputObject $spec -Depth 10 | Set-Content -LiteralPath $credspec -Encoding utf8NoBOM -Force

            $containerName = "ccgtest_${source}"

            & docker run --user "NT AUthority\System" --security-opt "credentialspec=file://${credspecfile}" --name "$containerName" --rm -d "$ContainerImage" cmd /c ping -t localhost

            if (-not $?) {
                throw "Error runnung docker command."
            }

            function Wait-WinEvent {
                [CmdletBinding()]
                param(
                    [Parameter(Mandatory)]
                    [ValidateNotNullOrEmpty()]
                    [String]
                    $LogName ,

                    [Parameter()]
                    [UInt16]
                    $EventID ,

                    [Parameter()]
                    [ScriptBlock]
                    $Condition ,

                    [Parameter()]
                    [DateTime]
                    $MinimumDateTime ,

                    [Parameter()]
                    [UInt16]
                    $TimeoutSeconds ,

                    [Parameter()]
                    [UInt16]
                    $SleepMilliseconds = 250
                )

                End {
                    $delay = $false
                    $timeoutStart = [DateTime]::Now
                    do {
                        if ($delay) {
                            Start-Sleep -Milliseconds $SleepMilliseconds
                        }
                        $ev = Get-WinEvent -LogName $LogName -FilterXPath "*[System[EventID=${EventID}]]" -MaxEvents 1 -ErrorAction SilentlyContinue
                        $delay = $delay -or $SleepMilliseconds -as [bool]
                    } until (
                        ($null -eq $MinimumDateTime -or $ev.TimeCreated -gt $MinimumDateTime) -and
                        ($null -eq $Condition -or (ForEach-Object -InputObject $ev -Process $Condition)) -or
                        (-not $TimeoutSeconds -or ($timedOut = ([DateTime]::Now - $timeoutStart).TotalSeconds -gt $TimeoutSeconds))
                    )

                    if (-not $timedOut) {
                        $ev
                    }
                }
            }
        }

        It "The plugin was instantiated" {
            $event1 = Wait-WinEvent -LogName $logname -EventId 1 -Condition {
                [guid]::Parse($_.Properties[0].Value) -eq $ccgPluginId
            } -MinimumDateTime $time -TimeoutSeconds $timeout

            $event1 | Should -Not -BeNullOrEmpty
        }

        # EventID 8 gets logged on my local machine when used with test creds
        # and the contoso.com domain, but it doesn't happen in CI nor in a test
        # server; maybe this is because my machine is a desktop OS, maybe it's
        # because it's domain-joined, not sure. Will keep it around as a possible
        # future test-case.
        #
        # It "The credential fails without a real domain" -Tag NoDomain {
        #     $event8 = Wait-WinEvent -LogName $logname -EventId 8 -Condition {
        #         [guid]::Parse($_.Properties[1].Value) -eq $ccgPluginId
        #     } -MinimumDateTime $time -TimeoutSeconds $timeout

        #     $event8 | Should -Not -BeNullOrEmpty
        # }

        AfterAll {
            & docker stop "$containerName"
            Remove-Item -LiteralPath $credspec -Force
        }
    }
}
