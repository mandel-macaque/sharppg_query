using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Frosting;

[TaskName("RunTests")]
[TaskDescription("Runs the xUnit test suite.")]
[IsDependentOn(typeof(BuildTask))]
public sealed class RunTestsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetTest("SharpPgQuery.slnx", new DotNetTestSettings
        {
            Configuration = context.MsBuildConfiguration,
            NoRestore     = true,
            NoBuild       = true,
            Loggers       = new[] { "trx" },
        });
    }
}
