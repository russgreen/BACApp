using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using System;
using System.Linq;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;

partial class Build : NukeBuild
{
    readonly AbsolutePath OutputDirectory = RootDirectory / "output";
    readonly AbsolutePath SourceDirectory = RootDirectory / "source";

    readonly string[] CompiledAssemblies = { "BACApp.Core.dll", "BACApp.UI.exe", "FireStopper.RulesEditor.exe" };

    [GitRepository]
    [Required]
    readonly GitRepository GitRepository;

    [Solution(GenerateProjects = true)]
    Solution Solution;

    public static int Main () => Execute<Build>(x => x.Compile);

    //[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

}
