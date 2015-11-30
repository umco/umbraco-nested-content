param($installPath, $toolsPath, $package, $project)

$appPluginsFolder = $project.ProjectItems | Where-Object { $_.Name -eq "App_Plugins" }
$nestedContentFolder = $appPluginsFolder.ProjectItems | Where-Object { $_.Name -eq "NestedContent" }

if (!$nestedContentFolder)
{
	$newPackageFiles = "$installPath\Content\App_Plugins\NestedContent"

	$projFile = Get-Item ($project.FullName)
	$projDirectory = $projFile.DirectoryName
	$projectPath = Join-Path $projDirectory -ChildPath "App_Plugins"
	$projectPathExists = Test-Path $projectPath

	if ($projectPathExists) {	
		Write-Host "Updating Nested Content App_Plugin files using PS as they have been excluded from the project"
		Copy-Item $newPackageFiles $projectPath -Recurse -Force
	}
}
