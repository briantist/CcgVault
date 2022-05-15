#Requires -Module @{ ModuleName = 'Pester'; ModuleVersion = '5.2.0' }

BeforeAll {
    $data = Import-PowerShellDataFile -LiteralPath $PSScriptRoot\TestData.psd1

    $ccgPluginId = [Guid]::Parse($data.ComPlus.CcgPlugin.ID)

    $Script:path = $data.Registry.Path | Join-Path -ChildPath $ccgPluginId.ToString('B')
}

Describe 'Registry tests' -Tag Registry {
    Context 'Present' -Tag Present {
        It 'Plugin CLSID is enabled for CCG use' {
            Test-Path -LiteralPath $path | Should -BeTrue
        }
    }

    Context 'Absent' -Tag Absent {
        It 'Plugin CLSID is not enabled for CCG use' {
            Test-Path -LiteralPath $path | Should -Not -BeTrue
        }
    }
}
