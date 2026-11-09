# Survival Log 模组工具包

[English](README.md)

本页说明如何让你的 Survival Log 模组支持 [Mod Manager](https://www.nexusmods.com/survivallog/mods/73)，让玩家在游戏内的模组界面里安装和更新它，不用手动下载。

只需要在项目文件里加几行，并在每次 GitHub 发布时附上一个 zip。你的模组不需要引用 Mod Manager，没有它也照常运行。

## 玩家能得到什么

- **检查更新**：模组界面会显示你的模组有新版本，并附上更新说明。
- **「安装更新」**：一键把新版本放好，下次启动游戏时生效，旧版本会保留以便回滚。
- **在「浏览」页安装**：你的模组上了列表后（第 5 步），就能在「浏览」页直接安装。「浏览」页列出 Nexus Mods 上所有 Survival Log 模组，没有设置好的模组会标为需要手动下载。

## 1. 写上你的 GitHub 仓库

在 .csproj 里加入以下内容，换成你自己的 `owner/repository`：

```xml
<ItemGroup>
  <AssemblyMetadata Include="ModManager.GitHub" Value="your-name/SurvivalLog-BetterMap" />
  <AssemblyMetadata Include="ModManager.NexusId" Value="47" />
  <AssemblyMetadata Include="ModManager.GameVersion" Value="1.1.20719" />
</ItemGroup>
```

- `ModManager.GitHub` 开启检查更新和安装。如果 .csproj 已经把 `RepositoryUrl` 设为这个 GitHub 仓库，可以不写。
- `ModManager.NexusId` 是你的 Nexus 页面地址最后的数字（`nexusmods.com/survivallog/mods/<数字>`）。
- `ModManager.GameVersion` 是模组所针对的游戏版本。

中文名称和简介、图标、分类、快捷键、即时生效的设置等更多键，见 Mod Manager 的 [For mod authors](https://github.com/AnExiledDev/SurvivalLog-ModManager#for-mod-authors) 部分。

## 2. 三个版本号保持一致

.csproj 里的 `<Version>`、`[BepInPlugin]` 里的版本号，以及发布标签（`v1.0.0` 或 `1.0.0`）。Mod Manager 用标签和已安装插件的版本号比较，不一致的话，要么一直提示有更新，要么永远检测不到更新。

发布之后不要再改 BepInPlugin 的 GUID，Mod Manager 靠它认出你的模组。

## 3. 生成发布用的 zip

把下面的 target 复制到 .csproj 里。之后每次 Release 构建都会额外生成 `bin\Release\<模组名>-<版本>.zip`，里面是 `BepInEx\plugins\<模组名>.dll`，和玩家解压到游戏文件夹的结构一样：

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

[example/BetterMap](example/BetterMap) 是包含以上全部内容的完整项目。

如果模组不止一个文件，请放在单独的文件夹里（`BepInEx\plugins\BetterMap\`），并把其他文件也放进 zip 里的这个文件夹。在共用的 `BepInEx\plugins` 文件夹里，只会安装你的 DLL 及其 `.pdb` 和 `.xml`。

## 4. 在 GitHub 上发布

用 Release 构建，然后把 zip 附到一个以版本号为标签的发布上。在网页上：打开你的仓库，点 Releases，再点 Draft a new release。或者用 [GitHub CLI](https://cli.github.com/)：

```sh
dotnet build -c Release
gh release create v1.0.0 bin/Release/BetterMap-1.0.0.zip --title "1.0.0" --notes "本次更新的内容。"
```

- 每个发布的说明只写这一版的改动。玩家会看到从他们的版本到最新版之间每一版的说明。
- 草稿和预发布会被跳过。
- Nexus 照常上传。Mod Manager 看的是 GitHub，玩家找模组是在 Nexus。

构建要在你自己的电脑上进行，因为需要游戏的 DLL，而这些不能放到 GitHub 上。

## 5. 加入「浏览」列表

只要有带 zip 的发布，更新就能用。要在「浏览」页显示「安装」按钮，Mod Manager 需要知道哪个 Nexus 页面对应哪个模组，所以请[在这里开一个 issue](../../issues/new?template=get-listed.yml)，写上：

- 你的 BepInPlugin GUID
- 你的 GitHub 仓库（`owner/repository`）
- 你的 DLL 文件名
- 你的 Nexus 页面编号（如果模组在 Nexus 上）

上了列表之后，「浏览」页就会显示你的模组和「安装」按钮。

## Mod Manager 会检查什么

为了让玩家放心一键安装，每次下载都会先检查：

- 只从 GitHub 通过 https 下载，最大 50 MB。
- zip 里的 DLL 必须声明和已安装版本相同的 BepInPlugin GUID。
- zip 里如果有文件会落到模组文件夹之外，或者有 Windows 打开时会运行的文件（`.exe`、`.bat`、`.ps1`、`.js` 等），会被拒绝。
- 在「浏览」页首次安装时，如果 DLL 能连网、启动程序、从字节加载代码、读注册表、调用本机代码或打开网页，会先告诉玩家它能做什么，等玩家点「仍然安装」。
- 更新时，如果新版本开始启动程序、从字节加载代码或读注册表，而已安装的版本没有，会被拒绝，玩家需要手动安装。如果开始连网、调用本机代码、打开网页或在你的 DLL 旁边新增 DLL，会告诉玩家并等玩家同意。这类改动请在更新说明里写一句原因。

## 游戏更新

游戏更新后如果你的模组仍然能用，不用发新版本也能说明：在仓库默认分支的根目录放一个 `game-versions.txt`。

```text
# mod version = game versions it is known to work on
1.0.0 = 1.1.20719, 1.1.21003
```

## 问题

在这里开 issue，或到 Core Directive Discord 提问：https://discord.gg/trTsyxUFqg
