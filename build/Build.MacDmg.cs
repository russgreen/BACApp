using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static Nuke.Common.IO.PathConstruction;

partial class Build
{
    Target MacDmg => _ => _
        .TriggeredBy(MacBundle)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .OnlyWhenStatic(() => OperatingSystem.IsMacOS())
        .Executes(() =>
        {
            foreach (var configuration in Solution.GetModel().BuildTypes.Where(x => x.StartsWith("Release", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var rid in MacRuntimes)
                {
                    var ridDir = OutputDirectory / "publish" / configuration / rid;
                    var appPath = ridDir / $"{MacAppName}.app";

                    if (!Directory.Exists(appPath))
                        throw new Exception($"App bundle not found: {appPath}");

                    var dmgTemp = ridDir / $"{MacAppName}-temp.dmg";
                    var dmgOut = ridDir / $"{MacAppName}_{configuration}.dmg";
                    var zipOut = ridDir / $"{MacAppName}_{configuration}.zip";
                    var volumeName = MacAppName;

                    if (File.Exists(dmgTemp)) File.Delete(dmgTemp);
                    if (File.Exists(dmgOut)) File.Delete(dmgOut);
                    if (File.Exists(zipOut)) File.Delete(zipOut);

                    RunProcess("chmod", $"+x \"{appPath / "Contents" / "MacOS" / MacAppName}\"", RootDirectory);

                    RunProcess(
                        "hdiutil",
                        $"create -srcfolder \"{appPath}\" -volname \"{volumeName}\" -fs HFS+ -format UDRW \"{dmgTemp}\"",
                        RootDirectory);

                    RunProcess(
                        "hdiutil",
                        $"convert \"{dmgTemp}\" -format UDBZ -o \"{dmgOut}\"",
                        RootDirectory);

                    File.Delete(dmgTemp);

                    Log.Information("Created DMG: {Dmg}", dmgOut);

                    // ZIP (preserves executable bits on macOS)
                    // --keepParent ensures the .app folder is preserved in the zip root, not only its contents
                    RunProcess(
                        "ditto",
                        $"-c -k --keepParent \"{appPath}\" \"{zipOut}\"",
                        RootDirectory);

                    Log.Information("Created ZIP: {Zip}", zipOut);
                }
            }
        });

    static int RunProcess(string fileName, string arguments, string workingDirectory = null, int timeoutMs = -1)
{
    var psi = new ProcessStartInfo(fileName, arguments)
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    };

    using var proc = Process.Start(psi);
    //stdout = proc.StandardOutput.ReadToEnd();
    //stderr = proc.StandardError.ReadToEnd();

    if (timeoutMs > 0)
    {
        if (!proc.WaitForExit(timeoutMs))
        {
            try { proc.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException($"Process {fileName} timed out.");
        }
    }
    else
    {
        proc.WaitForExit();
    }

    return proc.ExitCode;
}
}
