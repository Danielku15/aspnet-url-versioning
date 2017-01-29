param(
[String] $Configuration = "Release",
[String] $VersionStatus = ""
)

Set-Location (Split-Path $MyInvocation.MyCommand.Path  -Parent)

if ( !(Test-Path NuGet) ) {
	New-Item NuGet -ItemType Directory
} else {
	Get-ChildItem NuGet -Filter *.nupkg | Remove-Item -Force
}

$VersionSuffix = ""

if ( ![String]::IsNullOrEmpty($VersionStatus)) {
 $VersionSuffix = "--version-suffix"
}

dotnet pack src\CoderLine.AspNet.WebApi.Versioning  --configuration $Configuration --output NuGet $VersionSuffix $VersionStatus