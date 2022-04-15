whoami /priv
$ComServerGuid = '2D79156B-1DE7-4A29-9428-7B7A591B176E'
Get-CimInstance -ClassName Win32_Process -Filter "Name='dllhost.exe'" | select Name,ProcessId,CommandLine
Get-CimInstance -ClassName Win32_Process -Filter "Name='dllhost.exe' AND CommandLine LIKE '%${ComServerGuid}%'" -OutVariable process
$process
$process | Invoke-CimMethod -MethodName Terminate
