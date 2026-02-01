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
    Target MacNotarize => _ => _
            .TriggeredBy(MacDmg)
            .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
            .OnlyWhenStatic(() => OperatingSystem.IsMacOS())
            .Executes(() =>
            {
                foreach (var configuration in Solution.GetModel().BuildTypes.Where(x => x.StartsWith("Release", StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var rid in MacRuntimes)
                    {
                        var ridDir = OutputDirectory / rid;
                        var appPath = ridDir / $"{MacAppName}.app";
                        var dmgOut = ridDir / $"{MacAppName}_{rid}.dmg";
                        var zipOut = ridDir / $"{MacAppName}_{rid}.zip";

                        if (!File.Exists(zipOut))
                        {
                            throw new Exception($"ZIP file not found: {zipOut}");
                        }

                        if (!Directory.Exists(appPath))
                        {
                            throw new Exception($"App bundle not found: {appPath}");
                        }

                        Log.Information("Notarizing ZIP: {Zip}", zipOut);

                        var (notarizeExit, notarizeStdout, notarizeStderr) = RunProcessCapture(
                            "xcrun",
                            $"notarytool submit \"{zipOut}\" --keychain-profile \"AC_PASSWORD\" --wait",
                            RootDirectory);

                        if (notarizeExit != 0)
                        {
                            Log.Error("Notarization stdout: {Stdout}", notarizeStdout);
                            Log.Error("Notarization stderr: {Stderr}", notarizeStderr);
                            throw new Exception($"Notarization failed for {zipOut}");
                        }

                        Log.Information("Successfully notarized: {Zip}", zipOut);
                        Log.Information("Notarization output: {Output}", notarizeStdout);

                        // Wait for notarization ticket to be available (can take 1-2 minutes)
                        Log.Information("Waiting for notarization ticket to be available...");
                        System.Threading.Thread.Sleep(10000); // Wait 10 seconds for ticket to propagate

                        // Staple the ticket to the app bundle
                        Log.Information("Stapling app bundle: {App}", appPath);
                        var (stapleAppExit, stapleAppStdout, stapleAppStderr) = RunProcessCapture(
                            "xcrun",
                            $"stapler staple \"{appPath}\"",
                            RootDirectory);

                        if (stapleAppExit != 0)
                        {
                            Log.Error("Staple stdout: {Stdout}", stapleAppStdout);
                            Log.Error("Staple stderr: {Stderr}", stapleAppStderr);
                            throw new Exception($"Stapling failed for app bundle: {appPath}");
                        }

                        Log.Information("Stapled app bundle: {App}", appPath);
                    }
                }
            });
}
