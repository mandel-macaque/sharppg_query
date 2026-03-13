using System;
using Cake.Common;
using Cake.Core.IO;
using Cake.Frosting;

[TaskName("Docs")]
[TaskDescription("Generates the API documentation using DocFX.")]
[IsDependentOn(typeof(RestoreTask))]
public sealed class DocsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var docfxConfig = context.Environment.WorkingDirectory.CombineWithFilePath("docs/docfx.json");

        int exitCode = context.StartProcess("dotnet", new ProcessSettings
        {
            Arguments = $"docfx \"{docfxConfig.FullPath}\"",
            WorkingDirectory = context.Environment.WorkingDirectory
        });

        if (exitCode != 0)
        {
            throw new Exception($"DocFX exited with code {exitCode}.");
        }
    }
}
