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
        .WithProperty("PackageOutputPath", MakeAbsolute((DirectoryPath)"./output/").FullPath)
        .WithProperty("GeneratePackageOnBuild", "true"));
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    Warning("NOT YET RUNNING!");
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
