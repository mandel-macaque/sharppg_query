using Cake.Frosting;

[TaskName("Default")]
[TaskDescription("Default target: runs tests.")]
[IsDependentOn(typeof(RunTestsTask))]
public sealed class DefaultTask : FrostingTask
{
}
