#load "nuget:Dotnet.Build, 0.3.8"
using static FileUtils;
using System.Xml.Linq;

var owner = "seesharper";
var projectName = "LightInject.Microsoft.DependencyInjection";
var root = FileUtils.GetScriptFolder();
var solutionFolder = Path.Combine(root,"..","src");
var projectFolder = Path.Combine(root, "..", "src", projectName);

var testProjectFolder = Path.Combine(root, "..", "src", $"{projectName}.Tests");

var pathToTestAssembly = Path.Combine(testProjectFolder, "bin","release", "netcoreapp2.1", $"{projectName}.Tests.dll");
	

var artifactsFolder = CreateDirectory(root, "Artifacts");
var gitHubArtifactsFolder = CreateDirectory(artifactsFolder, "GitHub");
var nuGetArtifactsFolder = CreateDirectory(artifactsFolder, "NuGet");

var pathToReleaseNotes = Path.Combine(gitHubArtifactsFolder, "ReleaseNotes.md");

var version = ReadVersion();

var pathToGitHubReleaseAsset = Path.Combine(gitHubArtifactsFolder, $"{projectName}.{version}.zip");

string ReadVersion()
{
    var projectFile = XDocument.Load(Directory.GetFiles(projectFolder, "*.csproj").Single());
    var versionPrefix = projectFile.Descendants("VersionPrefix").SingleOrDefault()?.Value;
    var versionSuffix = projectFile.Descendants("VersionSuffix").SingleOrDefault()?.Value;
	var version = projectFile.Descendants("Version").SingleOrDefault()?.Value;

	if (version != null)
	{
		return version;
	}


    if (versionSuffix != null)
    {
        return $"{versionPrefix}-{versionSuffix}";
    }
    else
    {
        return versionPrefix;
    }
}