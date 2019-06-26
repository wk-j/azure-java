#addin "wk.StartProcess"
#addin "wk.ProjectParser"

using PS = StartProcess.Processor;
using ProjectParser;

var nugetToken = EnvironmentVariable("npi");
var name = "DotNetUpdate";

var currentDir = new DirectoryInfo(".").FullName;
var publishDir = ".publish";
var version = DateTime.Now.ToString("yy.MM.dd.HHmm");

Task("Pack").Does(() => {
    var settings = new DotNetCoreMSBuildSettings();
    settings.Properties["Version"] = new string[] { version };

    CleanDirectory(publishDir);
    DotNetCorePack($"src/{name}", new DotNetCorePackSettings {
        OutputDirectory = publishDir,
        MSBuildSettings = settings
    });
});

Task("Publish-NuGet")
    .IsDependentOn("Pack")
    .Does(() => {
        var nupkg = new DirectoryInfo(publishDir).GetFiles("*.nupkg").LastOrDefault();
        var package = nupkg.FullName;
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = nugetToken
        });
});

Task("Compile").Does(() => {
    PS.StartProcess($"mvn clean compile test-compile -Dversion={version}");
});

Task("Create-Jar")
    .IsDependentOn("Compile")
    .Does(() => {
        PS.StartProcess($@"mvn assembly:assembly -DdescriptorId=jar-with-dependencies -Dv={version}");

        var path = "publish";
        CreateDirectory(path);
        CleanDirectory(path);
        CopyFiles("target/*.jar", path);
    });

Task("Run-Jar")
    .Does(() => {
        PS.StartProcess($@"java -Dfile.encoding=UTF-8 -jar target/azure-java-{version}-jar-with-dependencies.jar 9090");
});

var target = Argument("target", "Pack");
RunTarget(target);
