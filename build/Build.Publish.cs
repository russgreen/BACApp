using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var configuration in Solution.GetModel().BuildTypes)
            {
                Log.Information("Configuration name: {configuration}", configuration);

                if (configuration.StartsWith("Release"))
                {

                }
            }
        });


}


