#! "netstandard1.6"
#r "nuget:NetStandard.Library,1.6.1"
#r "nuget:System.IO,4.3.0"
#r "nuget:System.Xml.XmlDocument,4.3.0"
#load "common.csx"
#load "logging.csx"

Log.Create("Main").Info("Build starting");

DotNet.Build(@"..\src\LightInject.Microsoft.DependencyInjection\LightInject.Microsoft.DependencyInjection.csproj");
DotNet.Build(@"..\src\LightInject.Microsoft.DependencyInjection.Tests\LightInject.Microsoft.DependencyInjection.Tests.csproj");
DotNet.Test(@"..\src\LightInject.Microsoft.DependencyInjection.Tests\LightInject.Microsoft.DependencyInjection.Tests.csproj");
DotNet.Pack(@"..\src\LightInject.Microsoft.DependencyInjection\LightInject.Microsoft.DependencyInjection.csproj");


