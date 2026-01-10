using Nuke.Common;
using Nuke.Common.IO;

partial class Build
{
    Target Sign => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
        });
}


