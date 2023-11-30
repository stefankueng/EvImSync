$filePathSrc = $PSScriptRoot + "/AssemblyInfo.tmpl"
$filePathDest = $PSScriptRoot + "/AssemblyInfo.cs"
Push-Location -Path $PSScriptRoot\..\..
try{
    $logCount = git rev-list --all --count
}
catch{
    $logCount = 0
}
Pop-Location
Write-Output "Updating AssemblyInfo.cs with log count: $logCount"
(Get-Content $filePathSrc).Replace("%WCLOGCOUNT%",$logCount) | Set-Content $filePathDest
