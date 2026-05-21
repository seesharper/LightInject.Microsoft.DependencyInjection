#load "nuget:Dotnet.Build, 0.29.0"
#load "nuget:dotnet-steps, 0.0.2"

BuildContext.CodeCoverageThreshold = 90;

[StepDescription("Runs the tests with test coverage")]
AsyncStep testcoverage = async () => await DotNet.TestWithCodeCoverageAsync();

[StepDescription("Runs all the tests for all target frameworks")]
AsyncStep test = async () => await DotNet.TestAsync();

[StepDescription("Creates the NuGet packages")]
AsyncStep pack = async () =>
{
    await test();
    await testcoverage();
    DotNet.Pack();
};

[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep deploy = async () =>
{
    await pack();
    await Artifacts.Deploy();
};

await StepRunner.Execute(Args);
return 0;

