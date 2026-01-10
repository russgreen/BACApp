using Nuke.Common;
using Nuke.Common.IO;

partial class Build
{
    Target Installer => _ => _
        .DependsOn(Sign)
        .Executes(() =>
        {
        });
}


