#load "nuget:Dotnet.Build, 0.16.1"
#load "nuget:dotnet-steps, 0.0.2"

[StepDescription("Runs the tests with test coverage")]
Step testcoverage = () => DotNet.TestWithCodeCoverage();

[StepDescription("Runs all the tests for all target frameworks")]
Step test = () => DotNet.Test();

[StepDescription("Creates the NuGet packages")]
Step pack = () =>
{
    test();
    testcoverage();
    DotNet.Pack();
};

[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep deploy = async () =>
{
    pack();
    await Artifacts.Deploy();
};

await StepRunner.Execute(Args);
return 0;

