using Cake.Core;
using Cake.Frosting;

/// <summary>
/// Shared build context that carries settings across all Cake Frosting tasks.
/// </summary>
public sealed class BuildContext : FrostingContext
{
    /// <summary>The MSBuild configuration (e.g. Debug / Release).</summary>
    public string MsBuildConfiguration { get; }

    /// <summary>The directory where NuGet packages are written.</summary>
    public string ArtifactsDir { get; }

    public BuildContext(ICakeContext context) : base(context)
    {
        MsBuildConfiguration = context.Arguments.GetArgument("configuration") ?? "Release";
        ArtifactsDir         = context.Arguments.GetArgument("artifacts")     ?? "./artifacts";
    }
}
