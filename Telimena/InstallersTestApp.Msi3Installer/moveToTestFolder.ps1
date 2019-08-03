param ([string]$TargetPath, [string]$SolutionDir)

Write-Host "Move to test folder starting..."
$fileName = [System.IO.Path]::GetFileName($TargetPath)

Write-Host "Running query on" $fileName

        $WindowsInstaller = New-Object -ComObject WindowsInstaller.Installer
        $MSIDatabase = $WindowsInstaller.GetType().InvokeMember("OpenDatabase", "InvokeMethod", $null, $WindowsInstaller, @($fileName, 0))
        $Query = "SELECT Value FROM Property WHERE Property = 'ProductVersion'"
        $View = $MSIDatabase.GetType().InvokeMember("OpenView", "InvokeMethod", $null, $MSIDatabase, ($Query))
        $View.GetType().InvokeMember("Execute", "InvokeMethod", $null, $View, $null)
        $Record = $View.GetType().InvokeMember("Fetch", "InvokeMethod", $null, $View, $null)
        $Value = $Record.GetType().InvokeMember("StringData", "GetProperty", $null, $Record, 1)


Write-Host "Version: "  $Value
 

Write-Host

$testAppsFolder = $SolutionDir+"Telimena.WebApp.AppIntegrationTests\Apps"
$finalPath = "$testAppsFolder\v$($Value)_$fileName"

Write-Host "Copying: "  $TargetPath "   To: $finalPath" 


Copy-Item -Path "$TargetPath" -Destination "$finalPath"


Write-Host "Copied" $zipPath1

