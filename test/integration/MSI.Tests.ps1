#Requires -Module @{ ModuleName = 'Pester'; ModuleVersion = '5.2.0' }
#Requires -Module @{ ModuleName = 'MSI'; ModuleVersion = '3.0.0' }

param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [String]
    $Name = 'CcgVault',

    [Parameter()]
    [System.IO.DirectoryInfo]
    $InstallPath ,

    [Parameter()]
    [System.Version]
    $ExpectedVersion = $env:CCG_BUILD_VER
)

BeforeAll {
    $Script:msi = Get-MSIProductInfo -Name $Name

    [System.IO.DirectoryInfo]$Script:path = if ($InstallPath) {
        $InstallPath
    }
    else {
        $env:ProgramFiles | Join-Path -ChildPath $Name
    }
}

Describe 'MSI tests' {
    Context 'Present' -Tag Present {
        It '<Name> is installed' {
            $msi | Should -Not -BeNullOrEmpty
        }

        It 'Only one version of <Name> is installed' {
            $msi | Should -HaveCount 1
        }

        It '<Name> should be version <ExpectedVersion>' {
            $msi.ProductVersion | Should -Be $ExpectedVersion
        }

        It 'Install directory "<path>" should be present' {
            $path.Exists | Should -BeTrue
        }
    }

    Context 'Absent' -Tag Absent {
        It '<Name> is not installed' {
            $msi | Should -BeNullOrEmpty
        }

        It 'Install directory should be absent' {
            $path.Exists | Should -BeFalse
        }
    }
}
