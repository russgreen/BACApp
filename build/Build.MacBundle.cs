using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Nuke.Common.IO.PathConstruction;

partial class Build
{
    static readonly string[] MacRuntimes =
    [
        "osx-x64",
        "osx-arm64"
    ];

    const string MacAppName = "BACApp.UI";
    const string MacExeName = "BACApp.UI"; // Avalonia app binary name on macOS
    const string MacIconFileName = "BAC.icns";

    Target MacBundle => _ => _
        .TriggeredBy(Publish)
        .OnlyWhenStatic(() => OperatingSystem.IsMacOS())
        .Executes(() =>
        {
            foreach (var configuration in Solution.GetModel().BuildTypes.Where(x => x.StartsWith("Release", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var rid in MacRuntimes)
                {
                    var publishDir = OutputDirectory / rid;

                    if (!Directory.Exists(publishDir))
                    {
                        throw new Exception($"Publish directory not found: {publishDir}");
                    }

                    var appBundleRoot = publishDir / $"{MacAppName}.app";
                    var contentsDir = appBundleRoot / "Contents";
                    var macOsDir = contentsDir / "MacOS";
                    var resourcesDir = contentsDir / "Resources";

                    appBundleRoot.CreateOrCleanDirectory();
                    macOsDir.CreateDirectory();
                    resourcesDir.CreateDirectory();

                    // Copy published output into Contents/MacOS (excluding the .app folder itself if it exists)
                    CopyDirectoryRecursivelyCustom(
                        sourceDir: publishDir,
                        destinationDir: macOsDir,
                        shouldCopy: path => !path.EndsWith(".app", StringComparison.OrdinalIgnoreCase));

                    // Copy app icon into Contents/Resources
                    var iconSourcePath = RootDirectory / MacIconFileName;
                    if (!File.Exists(iconSourcePath))
                    {
                        throw new Exception($"macOS icon not found at '{iconSourcePath}'. Place '{MacIconFileName}' in the solution root.");
                    }

                    var iconDestPath = resourcesDir / MacIconFileName;
                    File.Copy(iconSourcePath, iconDestPath, overwrite: true);


                    // Ensure main executable bit is set (if publishing on macOS; on Windows this is harmless)
                    var exePath = macOsDir / MacExeName;
                    if (!File.Exists(exePath))
                    {
                        // Fallback: sometimes the executable may be <AssemblyName> without extension, ensure it exists
                        var candidates = Directory.GetFiles(macOsDir, "*", SearchOption.TopDirectoryOnly)
                            .Select(x => (AbsolutePath)x)
                            .Where(x => Path.GetFileName(x).Equals(MacExeName, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (!candidates.Any())
                        {
                            throw new Exception($"Could not find macOS executable '{MacExeName}' in {macOsDir}");
                        }
                    }

                    // Write Info.plist
                    var infoPlistPath = contentsDir / "Info.plist";
                    File.WriteAllText(infoPlistPath, BuildInfoPlist(
                        bundleId: "com.russgreen.bacapp", // change if you have an official id
                        appName: MacAppName,
                        version: Solution.BACApp_UI.GetProperty("VersionPrefix") ?? "0.0.0",
                        executableName: MacExeName
                    ));


                    // Write PkgInfo
                    var pkgInfoPath = contentsDir / "PkgInfo";
                    File.WriteAllText(pkgInfoPath, "APPL????");

                    // If you want an icon later: add ICNS and set CFBundleIconFile.
                    Log.Information("Created app bundle: {AppBundle}", appBundleRoot);
                }
            }
        });

    static void CopyDirectoryRecursivelyCustom(
        AbsolutePath sourceDir,
        AbsolutePath destinationDir,
        Func<string, bool>? shouldCopy = null)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException(sourceDir);

        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDir, file);

            if (shouldCopy != null && !shouldCopy(relative))
                continue;

            var targetFile = Path.Combine(destinationDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            File.Copy(file, targetFile, overwrite: true);
        }
    }

    static bool ContainsPathSegmentEndingWithApp(string relativePath)
    {
        // Normalize separators then inspect segments
        var normalized = relativePath.Replace('\\', '/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Any(s => s.EndsWith(".app", StringComparison.OrdinalIgnoreCase));
    }

    static string BuildInfoPlist(string bundleId, string appName, string version, string executableName) =>
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>en</string>
  <key>CFBundleExecutable</key>
  <string>{executableName}</string>
  <key>CFBundleIdentifier</key>
  <string>{bundleId}</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>{appName}</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>{version}</string>
  <key>CFBundleVersion</key>
  <string>{version}</string>
  <key>LSMinimumSystemVersion</key>
  <string>11.0</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
";
}