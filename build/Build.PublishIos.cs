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

    Target PublishIos => _ => _
        .TriggeredBy(Compile)
        .OnlyWhenStatic(() => OperatingSystem.IsMacOS())
        .Executes(() =>
        {
            foreach (var configuration in Solution.GetModel().BuildTypes)
            {
                if (configuration.StartsWith("Release"))
                {
                    var publishDirectory = OutputDirectory / "ios";
                    publishDirectory.CreateOrCleanDirectory();
                    DotNetPublish(settings => settings
                        .SetProject(Solution.BACApp_iOS)
                        .SetConfiguration(configuration)
                        .SetRuntime("ios-arm64")
                        .SetOutput(publishDirectory)
                        .SetProperty("ArchiveOnBuild", "true")
                        .SetProperty("CodesignKey", "Apple Distribution: Russ Green (U5WY2DP4A6)")
                        .SetProperty("CodesignProvision", "BAC for CloudbaseGA App Store")
                        .SetVerbosity(DotNetVerbosity.minimal));
                }
            }


        });
}

