using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.IO;
using Serilog;
using System.Linq;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

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
                            .SetVerbosity(DotNetVerbosity.minimal));
                    }
                }
            }
        });


}


