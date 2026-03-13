///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target        = Argument("target",        "Default");
var configuration = Argument("configuration", "Release");
var artifactsDir  = Argument("artifacts",     "./artifacts");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Information("Building SharpPgQuery – target: {0}, configuration: {1}", target, configuration);
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories("src/**/bin");
        CleanDirectories("src/**/obj");
        CleanDirectory(artifactsDir);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetRestore("SharpPgQuery.slnx");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetBuild("SharpPgQuery.slnx", new DotNetBuildSettings
        {
            Configuration = configuration,
            NoRestore     = true,
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetTest("SharpPgQuery.slnx", new DotNetTestSettings
        {
            Configuration = configuration,
            NoRestore     = true,
            NoBuild       = true,
            Loggers       = new[] { "trx" },
        });
    });

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        EnsureDirectoryExists(artifactsDir);
        DotNetPack("src/SharpPgQuery/SharpPgQuery.csproj", new DotNetPackSettings
        {
            Configuration = configuration,
            NoRestore     = true,
            NoBuild       = true,
            OutputDirectory = artifactsDir,
        });
    });

Task("Publish")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

///////////////////////////////////////////////////////////////////////////////
// DEFAULT
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

RunTarget(target);
