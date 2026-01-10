using Nuke.Common;
using Nuke.Common.IO;

partial class Build
{
    Target Clean => _ => _
        .Before(Compile)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("*/bin", "*/obj").DeleteDirectories();
            OutputDirectory.CreateOrCleanDirectory();
        });
}


