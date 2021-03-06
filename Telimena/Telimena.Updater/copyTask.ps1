﻿param ([string]$TargetPath, [string]$SolutionDir)
$fileName = [System.IO.Path]::GetFileName($TargetPath)

Write-Host "Copy to apps folder starting..."
Write-Host "File Path: "  $TargetPath

$testAppsFolder = "$SolutionDir\Telimena.WebApp.UITests\02. IntegrationTests\Apps"

Copy-Item $TargetPath -Destination $testAppsFolder
Write-Host "Copied to $testAppsFolder"

try { 
    $zipPath = "$testAppsFolder\Updater.zip"

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
