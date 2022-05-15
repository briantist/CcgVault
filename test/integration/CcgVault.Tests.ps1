#Requires -Module @{ ModuleName = 'Pester'; ModuleVersion = '5.2.0' }

param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $CcgVault = 'C:\Program Files\CcgVault\CcgVault.exe' ,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $Config = 'C:\ccg\test.yml'
)

BeforeAll {
    $data = Import-PowerShellDataFile -LiteralPath $PSScriptRoot\TestData.psd1

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
}
