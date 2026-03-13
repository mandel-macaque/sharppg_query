using System;
using Cake.Common;
using Cake.Common.Tools.DotNet.Tool;
using Cake.Core.IO;
using Cake.Core.IO.Arguments;
using Cake.Frosting;

[TaskName("Docs")]
[TaskDescription("Generates the API documentation using DocFX.")]
[IsDependentOn(typeof(RestoreTask))]
public sealed class DocsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var docfxConfig = context.Environment.WorkingDirectory.CombineWithFilePath("docs/docfx.json");

        var settings = new DotNetToolSettings
        {
            WorkingDirectory = context.Environment.WorkingDirectory
        };

        var runner = new DotNetToolRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools);

        var restoreArgs = new ProcessArgumentBuilder();
        restoreArgs.Append(new TextArgument("restore"));
        runner.Execute(null, "tool", restoreArgs, settings);

        var runArgs = new ProcessArgumentBuilder();
        runArgs.Append(new TextArgument("run"));
        runArgs.Append(new TextArgument("docfx"));
        runArgs.Append(new TextArgument(docfxConfig.FullPath));
        runner.Execute(null, "tool", runArgs, settings);
    }
}
