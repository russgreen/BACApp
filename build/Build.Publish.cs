using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.IO;
using Serilog;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using System;

partial class Build
{
    static readonly string[] PublishRuntimes =
    [
        "osx-x64",
        "osx-arm64",
        "linux-x64",
        "linux-arm",
        "linux-arm64",
        "win-x64",
    ];

    Target Publish => _ => _
        .TriggeredBy(Compile)
        .Executes(() =>
        {
            var publishableProject = Solution.BACApp_UI;

            foreach (var configuration in Solution.GetModel().BuildTypes)
            {
                Log.Information("Configuration name: {configuration}", configuration);

                if (configuration.StartsWith("Release"))
                {
                    foreach (var runtime in PublishRuntimes)
                    {
                        var publishDirectory = OutputDirectory / "publish" / configuration / runtime;
                        publishDirectory.CreateOrCleanDirectory();

                        var publishSelfContained = true;
                        if(runtime == "win-x64")
                        {
                            publishSelfContained = false;
                        }

                        Log.Information("{runtime} is self contained: {selfContained}", runtime, publishSelfContained);

                        var isLinuxArmFamily = runtime is "linux-arm" or "linux-arm64";

                        DotNetPublish(settings => settings
                            .SetProject(Solution.BACApp_UI)
                            .SetConfiguration(configuration)
                            .SetRuntime(runtime)
                            .SetOutput(publishDirectory)
                            .SetSelfContained(publishSelfContained)
                            .SetProperty("PublishSingleFile", "true")
                            //.SetProperty("PublishTrimmed", "true")
                            .SetProperty("IncludeNativeLibrariesForSelfExtract", "true")
                            .SetProperty("DebugType", "none")
                            .SetProperty("DebugSymbols", "false")
                            // Avoid illegal-instruction issues on older ARM CPUs caused by native/AOT codegen.
                            .SetProperty("PublishAot", isLinuxArmFamily ? "false" : null)
                            .SetVerbosity(DotNetVerbosity.minimal));


                        if (isLinuxArmFamily)
                        {
                            var launcherPath = publishDirectory / "run-pi.sh";
                            launcherPath.WriteAllText(
                                "#!/usr/bin/env bash\n" +
                                "set -euo pipefail\n" +
                                "export AVALONIA_USE_SOFTWARE_RENDERING=1\n" +
                                "DIR=\"$(cd \"$(dirname \"$0\")\" && pwd)\"\n" +
                                "exec \"$DIR/BACApp.UI\" \"$@\"\n");

                            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                            {
                                ProcessTasks.StartProcess("chmod", $"+x \"{launcherPath}\"").AssertZeroExitCode();
                            }
                        }

                        if (runtime == "linux-arm")
                        {
                            var scriptSource = RootDirectory / "run-wayland.sh";
                            var scriptTarget = publishDirectory / "run-wayland.sh";

                            scriptTarget.DeleteFile();
                            System.IO.File.Copy(scriptSource, scriptTarget, true);
        
                            // Mark executable (best-effort; on Windows this is harmless/no-op depending on filesystem).
                            if(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                            {
                                ProcessTasks.StartProcess("chmod", $"+x \"{scriptTarget}\"").AssertZeroExitCode();
                            }
                            
                        }
                    }
                }
            }
        });


}


