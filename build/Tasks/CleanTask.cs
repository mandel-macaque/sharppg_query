using Cake.Common.IO;
using Cake.Frosting;

[TaskName("Clean")]
[TaskDescription("Removes all build output and artifact directories.")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.CleanDirectories("src/**/bin");
        context.CleanDirectories("src/**/obj");
        context.CleanDirectory(context.ArtifactsDir);
    }
}
