$filePathSrc = $PSScriptRoot + "/AssemblyInfo.tmpl"
$filePathDest = $PSScriptRoot + "/AssemblyInfo.cs"
Push-Location -Path $PSScriptRoot\..\..
$logCount = git rev-list --all --count
Pop-Location
(Get-Content $filePathSrc).Replace("%WCLOGCOUNT%",$logCount) | Set-Content $filePathDest
