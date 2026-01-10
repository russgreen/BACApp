using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;

partial class Build
{
    Target Installer => _ => _
        .TriggeredBy(Sign)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .Executes(() =>
        {
        });
}


