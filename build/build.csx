#load "nuget:Dotnet.Build, 0.3.8"
#load "nuget:github-changelog, 0.1.5"
#load "BuildContext.csx"
using static FileUtils;
using static xUnit;
using static DotNet;
using static ChangeLog;
using static ReleaseManagement;

Build(projectFolder, Git.Default.GetCurrentCommitHash());
Test(testProjectFolder);
Pack(projectFolder, nuGetArtifactsFolder);


if (BuildEnvironment.IsSecure)
    {
        await CreateReleaseNotes();

        if (Git.Default.IsTagCommit())
        {
            Git.Default.RequreCleanWorkingTree();
            await ReleaseManagerFor(owner, projectName,BuildEnvironment.GitHubAccessToken)
            .CreateRelease(Git.Default.GetLatestTag(), pathToReleaseNotes, Array.Empty<ReleaseAsset>());
            NuGet.TryPush(nuGetArtifactsFolder);            
        }
    }

private async Task CreateReleaseNotes()
{
    Logger.Log("Creating release notes");        
    var generator = ChangeLogFrom(owner, projectName, BuildEnvironment.GitHubAccessToken).SinceLatestTag();
    if (!Git.Default.IsTagCommit())
    {
        generator = generator.IncludeUnreleased();        
    }
    await generator.Generate(pathToReleaseNotes, FormattingOptions.Default.WithPullRequestBody());
}   