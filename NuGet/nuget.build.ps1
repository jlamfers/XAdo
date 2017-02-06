#param( [string] $version = $(throw "version argument is required") )
$strPath = [System.IO.Path]::GetFullPath($PSScriptRoot + "/XAdo.dll");
$Assembly = [Reflection.Assembly]::Loadfile($strPath)
$AssemblyName = $Assembly.GetName()
$Assemblyversion = $AssemblyName.version

&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/XAdo.nuspec -Version $Assemblyversion
&$PSScriptRoot"/nuget.exe" pack $PSScriptRoot/XAdo.SqlObjects.nuspec -Version $Assemblyversion