using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageVibrations.Core;

public class VintageVibrationsHarmony : ModSystem
{
    private Harmony? harmony;
    
    public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Client;
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        harmony = new Harmony(Mod.Info.ModID);
        harmony.PatchAll();
    }

    public override void Dispose()
    {
        harmony?.UnpatchAll(Mod.Info.ModID);
        harmony = null;
    }
}