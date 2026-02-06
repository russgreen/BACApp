## Plan: NetSparkle Cross-Platform Updater

Implement NetSparkleUpdater in the Avalonia UI app, using a per-platform appcast hosted on GitHub Pages, with signed update metadata and enclosures pointing at GitHub Release assets (MSI on Windows, notarized ZIP on macOS, tar.gz on Linux). The app will check once on startup and prompt if an update is available. We’ll also align the “version the user sees” (MSI/mac bundle/appcast) so NetSparkle compares the same version you ship.

**Steps**
1. Version alignment (avoid updater mismatches)
   - Standardize on a single version string used everywhere (assembly, MSI, mac Info.plist, appcast).
   - Update build targets that currently use `VersionPrefix` so they use the full `Version` (or a deliberately chosen “display vs update” version).
   - Files to touch: Directory.Build.props, build/Build.Installer.cs, build/Build.MacBundle.cs

2. Choose and document the update artifacts per OS (already mostly in place)
   - Windows: use the signed MSI produced by the existing `Installer` target (from `installer/BACApp-SetupFiles/*{version}.msi`) via build/Build.Installer.cs
   - macOS: use the notarized ZIP produced by `MacDmg`/`MacNotarize` (`output/osx-*/BACApp.UI_osx-*.zip`) via build/Build.MacDmg.cs and build/Build.MacNotarize.cs
   - Linux: add a new NUKE packaging step that creates `tar.gz` from each `output/linux-*` folder (portable, user-writable install assumption)

3. Add NetSparkle to the app and wire it via DI
   - Add the NetSparkle NuGet package(s) to the UI project (source/BACApp.UI/BACApp.UI.csproj)
   - Create a small “updater wrapper” service (singleton) and register it in Host.cs so it can use logging and configuration consistently
   - Store the appcast base URL in configuration:
     - Add an `appsettings.json` (picked up by `CreateDefaultBuilder()`) with something like “Updater:AppCastBaseUrl”
     - Ensure it’s copied to publish output (important because you publish single-file but still can ship sidecar config)

4. Initialize updater after the main window exists (startup check + prompt)
   - Trigger update initialization and “check once” from Avalonia lifetime initialization so dialogs can be owned by the window:
     - Hook in App.axaml.cs in `OnFrameworkInitializationCompleted()` (after `desktop.MainWindow` is created)
   - Keep Program.Main focused on single-instance and startup; avoid showing update UI from Program.cs

5. Make restart/update flow compatible with single-instance
   - Ensure whatever “restart to apply update” behavior NetSparkle uses won’t get blocked by the mutex/pipe logic:
     - Verify and (if needed) adjust handling in SingleInstanceCoordinator.cs and argument parsing in Program.cs
   - Goal: when updater relaunches, it should result in a real restarted primary instance (not just an activation ping)

6. Build automation: generate and publish appcasts
   - Add a new NUKE target that runs after packaging and produces appcast XML files (one per RID, e.g., `appcast-win-x64.xml`, `appcast-osx-arm64.xml`, etc.)
     - Source of truth for enclosures:
       - Windows MSI: from `installer/BACApp-SetupFiles`
       - mac ZIP: from `output/osx-*/BACApp.UI_osx-*.zip`
       - Linux tar.gz: from the new archive step
   - Host the appcasts on GitHub Pages; enclosures link to GitHub Release asset URLs
   - Enable NetSparkle appcast signing:
     - Generate a keypair; embed the public key in the app; store the private key in CI secrets (never in repo)

**Verification**
- Local build sanity:
  - Run `./build.ps1 Publish` and confirm `output/<rid>` contents match expectations (especially Linux folders and mac bundle inputs)
  - On macOS CI/host: run `./build.sh MacBundle MacSign MacDmg MacNotarize` and confirm the ZIP remains notarized/signed
  - On Windows: run `./build.ps1 Installer` and confirm MSI is produced and signed
- Updater behavior:
  - Host a test appcast + enclosure on a local/static server, bump version in Directory.Build.props, launch the app, and confirm it prompts exactly once on startup and installs the right artifact for the platform

**Decisions**
- Hosting: GitHub Releases (assets) + GitHub Pages (appcast XML)
- UX: check on startup + prompt
- Release channels: no stable/beta split (but still separate appcasts per platform/RID to keep enclosures unambiguous)
- Linux distribution: portable `tar.gz` extracted under user home (self-update supported)

If you want a single appcast file for all platforms (instead of per-RID URLs), say so and I’ll adjust the design to match what NetSparkle supports best for OS filtering.
