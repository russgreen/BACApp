using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using static Nuke.Common.IO.PathConstruction;
partial class Build
{
    // Prefer passing this in CI:
    // - nuke MacSign --mac-sign-identity "XXXXXXXXXX" (hash) or "Developer ID Application: ..."
    // Or set env var: MAC_SIGN_IDENTITY
    [Parameter("macOS codesign identity (hash or common name). If omitted, the build will try to resolve one from Keychain.")]
    readonly string? MacSignIdentity = Environment.GetEnvironmentVariable("MAC_SIGN_IDENTITY");

    Target MacSign => _ => _
        .TriggeredBy(MacBundle)
        .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
        .OnlyWhenStatic(() => OperatingSystem.IsMacOS())
        .Executes(() =>
        {
            var identity = ResolveMacCodeSignIdentity(MacSignIdentity);

            foreach (var configuration in Solution.GetModel().BuildTypes.Where(x => x.StartsWith("Release", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var rid in MacRuntimes)
                {
                    var ridDir = OutputDirectory / "publish" / configuration / rid;
                    var appPath = ridDir / $"{MacAppName}.app";
                    if (!Directory.Exists(appPath))
                    {
                        throw new Exception($"App bundle not found: {appPath}");
                    }

                    Log.Information("Signing macOS app bundle: {AppPath}", appPath);
                    Log.Information("Using codesign identity: {Identity}", identity);

                    // Sign (hardened runtime option is typical if you later notarize)
                    RunProcess(
                        "codesign",
                        $"--deep --force --options runtime --timestamp --verify --verbose --sign \"{identity}\" \"{appPath}\"",
                        RootDirectory);

                    // Verify signature
                    RunProcess(
                        "codesign",
                        $"--verify --deep --strict --verbose=4 \"{appPath}\"",
                        RootDirectory);

                    Log.Information("Successfully signed macOS app bundle: {AppPath}", appPath);
                }
            }
        });

    static string ResolveMacCodeSignIdentity(string? configuredIdentity)
    {
        if (!string.IsNullOrWhiteSpace(configuredIdentity))
            return configuredIdentity.Trim();

        // Query Keychain
        var (exitCode, stdout, stderr) = RunProcessCapture("security", "find-identity -v -p codesigning");
        if (exitCode != 0)
            throw new Exception($"Failed to query Keychain identities. ExitCode={exitCode}\n{stderr}");

        // Typical line:
        //  1) ABCDEF1234567890ABCDEF1234567890ABCDEF12 "Developer ID Application: Name (TEAMID)"
        var identityRegex = new Regex(
            @"^\s*\d+\)\s+(?<hash>[0-9A-F]{40})\s+""(?<name>[^""]+)""\s*$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        var matches = identityRegex.Matches(stdout);
        if (matches.Count == 0)
            throw new Exception("No code-signing identities found in Keychain (security find-identity returned none).");

        // Prefer Developer ID Application (distribution), then Apple Development (local dev).
        string? Pick(Func<string, bool> predicate) =>
            matches
                .Select(m => new { Hash = m.Groups["hash"].Value, Name = m.Groups["name"].Value })
                .Where(x => predicate(x.Name))
                .Select(x => x.Hash) // use hash for unambiguous selection
                .FirstOrDefault();

        var picked =
            Pick(n => n.StartsWith("Developer ID Application:", StringComparison.OrdinalIgnoreCase)) ??
            Pick(n => n.StartsWith("Apple Development:", StringComparison.OrdinalIgnoreCase));

        if (picked is null)
        {
            var available = string.Join("\n", matches.Select(m => $"- {m.Groups["name"].Value}"));
            throw new Exception($"No suitable signing identity found (expected Developer ID Application or Apple Development).\nAvailable:\n{available}");
        }

        return picked;
    }

    static (int ExitCode, string StdOut, string StdErr) RunProcessCapture(string fileName, string arguments, string? workingDirectory = null, int timeoutMs = -1)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var proc = Process.Start(psi) ?? throw new Exception($"Failed to start process: {fileName}");
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();

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

        return (proc.ExitCode, stdout, stderr);
    }
}
