using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Frosting;

[TaskName("RunTests")]
[TaskDescription("Builds the native library (if needed) then runs the xUnit test suite.")]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(BuildNativeTask))]
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
