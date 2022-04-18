#Requires -Module @{ ModuleName = 'Pester'; ModuleVersion = '5.2.0' }

param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $ServiceName = 'CcgVault'
)

BeforeAll {
    $data = Import-PowerShellDataFile -LiteralPath $PSScriptRoot\TestData.psd1
    $Script:service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

    $appId = [Guid]::Parse($data.ComPlus.ApplicationID)
    $Script:binaryPath = '{0} /ProcessId:{1}' -f @(
        (Get-Command -Type Application -Name $data.Service.Executable).Source
        $appId.ToString('B').ToUpper()
    )
}

Describe 'Service tests' {
    Context 'Present' -Tag Present {
        It 'Service "<ServiceName>" exists' {
            $service | Should -Not -BeNullOrEmpty
        }

        It 'Service runs as <data.ComPlus.Identity>' {
            $service.UserName | Should -Be $data.ComPlus.Identity
        }

        It 'Service starts the COM+ application' {
            $service.BinaryPathName | Should -Be $binaryPath
        }

        It 'Service depends on RPC' {
            $service.ServicesDependedOn.Count | Should -BeGreaterOrEqual 1
            $service.ServicesDependedOn.Name | Should -Contain rpcss
        }

        It 'Serice start mode is <data.Service.StartMode>' {
            $service.StartType | Should -Be $data.Service.StartMode
        }
    }

    Context 'Absent' -Tag Absent {
        It 'Service "<ServiceName>" does not exist' {
            $service | Should -BeNullOrEmpty
        }
    }
}
