using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

[TaskName("Build")]
[TaskDescription("Compiles the solution.")]
[IsDependentOn(typeof(RestoreTask))]
[IsDependentOn(typeof(BuildNativeTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild("SharpPgQuery.slnx", new DotNetBuildSettings
        {
            Configuration = context.MsBuildConfiguration,
            NoRestore     = true,
        });
    }
}
