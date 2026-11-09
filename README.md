# Survival Log mod kit

[中文说明](README.zh.md)

How to set up your Survival Log mod for the [Mod Manager](https://www.nexusmods.com/survivallog/mods/73), so players can install it and get its updates from the in-game Mods screen instead of downloading it by hand.

It takes a few lines in your project file and a zip on each GitHub release. Your mod does not reference the Mod Manager and runs the same without it.

## What players get

- **Update checks.** The Mods screen shows when a newer release of your mod is out, with its release notes.
- **INSTALL UPDATE.** One click puts the new release in place for the next game start, and the old one is kept for a rollback.
- **INSTALL on BROWSE.** Once your mod is on the list (step 5), it can be installed from the BROWSE page, which lists every Survival Log mod on Nexus Mods. Mods that are not set up are shown there as manual downloads.

## 1. Name your GitHub repository

Add this to your .csproj, with your own `owner/repository`:

```xml
<ItemGroup>
  <AssemblyMetadata Include="ModManager.GitHub" Value="your-name/SurvivalLog-BetterMap" />
  <AssemblyMetadata Include="ModManager.NexusId" Value="47" />
  <AssemblyMetadata Include="ModManager.GameVersion" Value="1.1.20719" />
</ItemGroup>
```

- `ModManager.GitHub` turns on update checks and installs. If your .csproj already sets `RepositoryUrl` to the GitHub repository, you can leave it out.
- `ModManager.NexusId` is the number at the end of your Nexus page address (`nexusmods.com/survivallog/mods/<number>`).
- `ModManager.GameVersion` is the game version you made the mod for.

There are more keys for a name and description in Chinese, an icon, a category, hotkeys and settings that apply live. They are all in the Mod Manager's [For mod authors](https://github.com/AnExiledDev/SurvivalLog-ModManager#for-mod-authors) section.

## 2. Keep three versions the same

The `<Version>` in your .csproj, the version in your `[BepInPlugin]`, and the release tag (`v1.0.0` or `1.0.0`). The Mod Manager compares the tag with the installed plugin's version, so a mismatch reads as an update that never goes away, or none at all.

Never change your BepInPlugin GUID after a release. It is how the Mod Manager knows your mod.

## 3. Make the release zip

Copy this target into your .csproj. Every Release build then also writes `bin\Release\<YourMod>-<version>.zip`, which holds `BepInEx\plugins\<YourMod>.dll`, the same layout players extract into the game folder:

```xml
<Target Name="ReleaseZip" AfterTargets="Build" Condition="'$(Configuration)' == 'Release'">
  <PropertyGroup>
    <ZipStage>$(MSBuildProjectDirectory)\obj\release-zip\</ZipStage>
  </PropertyGroup>
  <RemoveDir Directories="$(ZipStage)" />
  <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(ZipStage)BepInEx\plugins\" />
  <ZipDirectory SourceDirectory="$(ZipStage)" DestinationFile="$(OutDir)$(AssemblyName)-$(Version).zip" Overwrite="true" />
</Target>
```

[example/BetterMap](example/BetterMap) is a whole project with all of this in it.

If your mod ships more than one file, put it in its own folder (`BepInEx\plugins\BetterMap\`) and copy the other files into that folder in the zip. In the shared `BepInEx\plugins` folder, only your DLL and its `.pdb` and `.xml` are installed.

## 4. Publish the release on GitHub

Build in Release, then attach the zip to a release whose tag is the version. In the browser: your repository, Releases, Draft a new release. Or with the [GitHub CLI](https://cli.github.com/):

```sh
dotnet build -c Release
gh release create v1.0.0 bin/Release/BetterMap-1.0.0.zip --title "1.0.0" --notes "What changed in this release."
```

- Write each release's notes for that release alone. Players see the notes of every release between theirs and the newest.
- Drafts and prereleases are skipped.
- Keep uploading to Nexus as you do now. GitHub is where the Mod Manager looks; Nexus is where players find you.

The build runs on your own PC, since it needs the game's DLLs, which cannot be put on GitHub.

## 5. Get on the BROWSE list

Updates work as soon as a release with a zip is out. For INSTALL on BROWSE, the Mod Manager has to know which Nexus page is which mod, so [open an issue here](../../issues/new?template=get-listed.yml) with:

- your BepInPlugin GUID
- your GitHub repository (`owner/repository`)
- your DLL's file name
- your Nexus page number, if the mod is on Nexus

Once it is on the list, BROWSE shows your mod with an INSTALL button.

## What the Mod Manager checks

So players can trust a one-click install, each download is checked first:

- Downloads only come from GitHub over https, up to 50 MB.
- The DLL in the zip must declare the same BepInPlugin GUID as the installed one.
- A zip with a file that would land outside your mod's folder, or a file Windows runs when opened (`.exe`, `.bat`, `.ps1`, `.js` and the like), is refused.
- On a first install from BROWSE, a DLL that can connect to the internet, start a program, load code from bytes, read the registry, call native code or open web pages is shown to the player with what it can do, and waits for INSTALL ANYWAY.
- An update that starts a program, loads code from bytes or reads the registry when the installed version did not is refused, and players install it by hand. One that starts connecting to the internet, calling native code, opening web pages or adding a DLL beside yours is shown to the player and waits for them to say yes. Ship those changes with a line in the release notes saying why.

## Game updates

When the game updates and your mod still works, you can say so without a new release: put a `game-versions.txt` at the top of your repository's default branch.

```text
# mod version = game versions it is known to work on
1.0.0 = 1.1.20719, 1.1.21003
```

## Questions

Open an issue here, or ask on the Core Directive Discord: https://discord.gg/trTsyxUFqg
