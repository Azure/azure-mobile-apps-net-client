///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
{
    MSBuild("./Microsoft.Azure.Mobile.Client.sln", c => c
        .SetConfiguration(configuration)
        .EnableBinaryLogger("./output/build.binlog")
        .WithRestore()
        .WithTarget("Build")
        .WithProperty("PackageOutputPath", MakeAbsolute((DirectoryPath)"./output/").FullPath));
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = "Release",
        NoBuild = true,
        NoRestore = true,
        ResultsDirectory = "./output/unittests-results"
    };

    var failCount = 0;

    var projectFiles = GetFiles("./unittests/**/*.csproj");
    foreach(var file in projectFiles)
    {
        settings.Logger = "trx;LogFileName=" + file.GetFilenameWithoutExtension() + "-Results.trx";
        try
        {
            DotNetCoreTest(file.FullPath, settings);
        }
        catch
        {
            failCount++;
        }
    }

    if (failCount > 0)
        throw new Exception($"There were {failCount} test failures.");
});

///////////////////////////////////////////////////////////////////////////////
// ENTRYPOINTS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("ci")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

RunTarget(target);
