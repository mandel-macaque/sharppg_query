using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Frosting;

[TaskName("Pack")]
[TaskDescription("Builds the native library then creates the NuGet package.")]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(BuildNativeTask))]
public sealed class PackTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists(context.ArtifactsDir);
        context.DotNetPack("src/SharpPgQuery/SharpPgQuery.csproj", new DotNetPackSettings
        {
            Configuration   = context.MsBuildConfiguration,
            NoRestore       = true,
            NoBuild         = true,
            OutputDirectory = context.ArtifactsDir,
        });
    }
}
