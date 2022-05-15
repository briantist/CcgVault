#Requires -Module @{ ModuleName = 'Pester'; ModuleVersion = '5.2.0' }

param(
    [Parameter()]
    [ValidateNotNull()]
    [AllowEmptyString()]
    [String]
    $Identity = $env:COMPLUS_IDENTITY
)

BeforeAll {
    $data = Import-PowerShellDataFile -LiteralPath $PSScriptRoot\TestData.psd1

    if (-not $Identity) {
        $Identity = $data.ComPlus.Identity
    }

    $appId = [Guid]::Parse($data.ComPlus.ApplicationID)
    $ccgPluginId = [Guid]::Parse($data.ComPlus.CcgPlugin.ID)

    $cat = New-Object -ComObject ComAdmin.ComAdminCatalog
    $apps = $cat.GetCollection('Applications')
    $apps.Populate()

    $app =  $apps | Where-Object -FilterScript {
        $appId -eq $_.Key
    }

    if ($app) {
        $appProps = $apps.GetCollection('PropertyInfo', $app.Key)
        $appProps.Populate()

        $appInfo = @{}
        $appProps | ForEach-Object -Process {
            try {
                $appInfo[$_.Name] = $app.Value($_.Name)
            }
            catch {}
        }

        $components = $apps.GetCollection('Components', $app.Key)
        $components.Populate()

        $ccgPlugin = $components | Where-Object -FilterScript {
            $ccgPluginId -eq $_.Key
        }

        if ($ccgPlugin) {
            $compProps = $components.GetCollection('PropertyInfo', $ccgPlugin.Key)
            $compProps.Populate()

            $compInfo = @{}
            $compProps | ForEach-Object -Process {
                try {
                    $compInfo[$_.Name] = $ccgPlugin.Value($_.Name)
                }
                catch {}
            }
        }
    }
}

Describe 'COM+ tests' -Tag ComPlus {
    Context 'Present' -Tag Present {
        It 'There is exactly 1 app' {
            $app.Count | Should -BeExactly 1
        }

        It 'App name is CcgVault' {
            $app.Name | Should -BeExactly CcgVault
        }

        It 'App identity is <data.ComPlus.Identity>' {
            $appInfo.Identity | Should -Be $Identity
        }

        It 'App is enabled' {
            $appInfo.IsEnabled | Should -BeTrue
        }

        It 'App has exactly 1 component' {
            $ccgPlugin.Count | Should -BeExactly 1
        }

        It 'Component name is <data.ComPlus.CcgPlugin.Name>' {
            $ccgPlugin.Name | Should -BeExactly $data.ComPlus.CcgPlugin.Name
        }

        It 'Component is enabled' {
            $compInfo.IsEnabled | Should -BeTrue
        }
    }

    Context 'Absent' -Tag Absent {
        It 'App does not exist' {
            $app | Should -BeNullOrEmpty
        }
    }
}
