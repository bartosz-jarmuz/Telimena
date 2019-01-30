param ([string]$TargetPath, [string]$SolutionDir)

Write-Host "Copy to zip starting..."
$fileName = [System.IO.Path]::GetFileName($TargetPath)
Write-Host "Toolkit Path: "  $TargetPath "   FileName: $fileName" 
Write-Host
Write-Host

$testAppsFolder = "$SolutionDir\Telimena.WebApp.UITests\02. IntegrationTests\Apps"

Copy-Item $TargetPath -Destination $testAppsFolder


$zipPath1 = "$testAppsFolder\TestApp v1.0.0.0.zip"
$zipPath2 = "$testAppsFolder\AutomaticTestsClientv2.zip"
$zipPath3 = "$testAppsFolder\PackageTriggerUpdaterTestApp v.1.0.0.0.zip"
$zipPath4 = "$testAppsFolder\PackageTriggerUpdaterTestApp v.2.0.0.0.zip"



$zipPath1, $zipPath2, $zipPath3, $zipPath4| ForEach-Object {
	$zipPath = $_

	try { 
	[Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem') | Out-Null
	$zip = [System.IO.Compression.ZipFile]::Open($zipPath, "Update")

	$Entry = $zip.GetEntry($fileName)
	if ($Entry)
	{
		Write-Host "File exists in  $zipPath, it will be deleted and overwritten"
		$Entry.Delete()  
	} 
	 
	[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $TargetPath, $fileName,"Optimal") | Out-Null
	$zip.Dispose()
		Write-Host "Successfully added $fileName to $zipPath"
	} catch {
		Write-Warning "Failed to add $fileName to $zipPath. Details : $_"
	} 
	Write-Host ""
}