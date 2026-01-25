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
                        var ridDir = OutputDirectory / "publish" / configuration / rid;
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

                        // Submit for notarization
                        var notarizeOutput = RunProcess(
                            "xcrun",
                            $"notarytool submit \"{zipOut}\" --keychain-profile \"AC_PASSWORD\" --wait",
                            RootDirectory);

                        if (notarizeOutput != 0)
                        {
                            throw new Exception($"Notarization failed for {zipOut}");
                        }

                        Log.Information("Successfully notarized: {Zip}", zipOut);

                        // Staple the DMG if it exists (recommended for distribution)
                        if (File.Exists(dmgOut))
                        {
                            var stapleDmgExit = RunProcess(
                                "xcrun",
                                $"stapler staple \"{dmgOut}\"",
                                RootDirectory);

                            if (stapleDmgExit != 0)
                            {
                                throw new Exception($"Stapling failed for DMG: {dmgOut}");
                            }

                            Log.Information("Stapled DMG: {Dmg}", dmgOut);
                        }
                        else
                        {
                            Log.Warning("DMG not found, skipping staple: {Dmg}", dmgOut);
                        }
                    }
                }
            });
}
