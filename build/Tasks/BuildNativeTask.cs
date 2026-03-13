using System;
using System.Runtime.InteropServices;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

[TaskName("BuildNative")]
[TaskDescription("Clones libpg_query from source and builds the platform-specific shared library under native/{rid}/.")]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    private const string LibPgQueryRepo = "https://github.com/pganalyze/libpg_query.git";

    public override void Run(BuildContext context)
    {
        string rid, libName;
        bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        bool isOsx   = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        if (isLinux)
        {
            rid     = "linux-x64";
            libName = "libpg_query.so";
        }
        else if (isOsx)
        {
            rid     = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
            libName = "libpg_query.dylib";
        }
        else
        {
            context.Log.Warning("BuildNative is only supported on Linux and macOS. Skipping.");
            return;
        }

        var srcDir    = context.MakeAbsolute(context.Directory("native/src/libpg_query"));
        var nativeDir = context.MakeAbsolute(context.Directory($"native/{rid}"));
        var libPath   = nativeDir.CombineWithFilePath(libName);

        // Skip if the shared library is already built
        if (context.FileExists(libPath))
        {
            context.Log.Information("Native library already present at {0}. Skipping build.", libPath);
            return;
        }

        // Clone libpg_query source if not already checked out
        if (!context.DirectoryExists(srcDir))
        {
            context.EnsureDirectoryExists(srcDir.GetParent());
            Run(context, "git", $"clone --depth 1 {LibPgQueryRepo} \"{srcDir}\"");
        }
        else
        {
            context.Log.Information("libpg_query source already present at {0}. Skipping clone.", srcDir);
        }

        // Build the static archive
        Run(context, "make", workingDirectory: srcDir);

        // Link into a platform-specific shared library
        context.EnsureDirectoryExists(nativeDir);

        if (isLinux)
        {
            Run(context, "gcc",
                $"-shared -fPIC -o \"{libPath}\" -Wl,--whole-archive \"{srcDir}/libpg_query.a\" -Wl,--no-whole-archive -lm");
        }
        else
        {
            Run(context, "gcc",
                $"-dynamiclib -o \"{libPath}\" -Wl,-all_load \"{srcDir}/libpg_query.a\" -lm");
        }

        context.Log.Information("Native library written to: {0}", libPath);
    }

    private static void Run(BuildContext context, string tool, string? arguments = null,
                             DirectoryPath? workingDirectory = null)
    {
        var settings = new ProcessSettings();
        if (arguments        != null) settings.Arguments        = arguments;
        if (workingDirectory != null) settings.WorkingDirectory = workingDirectory;

        int exitCode = context.StartProcess(tool, settings);
        if (exitCode != 0)
            throw new Exception($"'{tool}' exited with code {exitCode}.");
    }
}
