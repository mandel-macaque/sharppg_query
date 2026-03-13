using Cake.Common.Tools.DotNet;
using Cake.Frosting;

[TaskName("Restore")]
[TaskDescription("Restores NuGet packages for the solution.")]
[IsDependentOn(typeof(CleanTask))]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore("SharpPgQuery.slnx");
    }
}
