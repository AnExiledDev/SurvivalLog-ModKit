using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace BetterMap;

// The GUID never changes once released: the Mod Manager uses it to know a mod across versions.
// The version is the same as <Version> in the .csproj and the release tag (v1.0.0).
[BepInPlugin("com.yourname.survivallog.bettermap", "Better Map", "1.0.0")]
public sealed class Plugin : BasePlugin
{
    public override void Load()
    {
        Log.LogInfo("Better Map loaded");
    }
}
