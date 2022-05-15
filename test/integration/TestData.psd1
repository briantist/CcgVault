@{
    ComPlus = @{
        ApplicationID = '{2D79156B-1DE7-4A29-9428-7B7A591B176E}'
        CcgPlugin = @{
            ID = '{01BF101D-BFB6-433F-B416-02885CDC5AD3}'
            Name = 'CcgCredProvider'
        }
        Identity = 'NT AUTHORITY\LocalService'
    }

    Service = @{
        StartMode = 'Manual'
        Executable = 'dllhost.exe'
    }

    Registry = @{
        Path = 'HKLM:\SYSTEM\CurrentControlSet\Control\CCG\COMClasses'
    }

    CcgVault = @{
        Kv1Data = @{
            Username = 'ccg-reader-1'
            Domain = 'contoso.com'
            Password = 'password1'
        }
        Kv2Data = @{
            Username = 'ccg-reader-2'
            Domain = 'contoso.com'
            Password = 'password2'
        }
    }
}
