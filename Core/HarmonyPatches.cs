using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace VintageVibrations.Core;

[HarmonyPatch]
internal partial class HarmonyPatches
{
    private static VintageVibrationsModSystem? GetVibrationsSystem(ICoreAPI api)
    {
        if(api is ICoreClientAPI capi)
            return capi.ModLoader.GetModSystem<VintageVibrationsModSystem>();
        return null;
    }

    private static bool IsLocalPlayer(ICoreAPI api, string playerUID)
    {
        if (api is ICoreClientAPI capi)
            return capi.World.Player.PlayerUID == playerUID;
        return false;
    }

    private static void TestMessage(ICoreAPI api, string msg = "test")
    {
        if (api is ICoreClientAPI capi)
            capi.ShowChatMessage(msg);
    }
    
    // damageSource is null on client
    [HarmonyPatch(typeof(EntityPlayer), "OnHurt")]
    [HarmonyPostfix]
    private static void OnHurt(DamageSource damageSource, float damage, EntityPlayer __instance) 
    {
        if (__instance.Api.Side != EnumAppSide.Client) return;
        if(!IsLocalPlayer(__instance.Api, __instance.PlayerUID)) return;
        
        const float minDamage = 0.5f;
        const float maxDamage = 21.0f;
        
        damage = Math.Clamp(damage, minDamage, maxDamage);
        float f = 0.1f + ((damage - minDamage) / (maxDamage - minDamage)) * 0.9f;
        
        GetVibrationsSystem(__instance.Api)?.VibrateWithDuration(f,0.5f);
    }

    [HarmonyPatch(typeof(BlockEntityKnappingSurface), "CheckIfFinished")]
    [HarmonyPrefix]
    private static void CheckIfFinished(IPlayer byPlayer, BlockEntityKnappingSurface __instance)
    {
        if (__instance.Api.Side != EnumAppSide.Client) return;
        if(!IsLocalPlayer(__instance.Api, byPlayer.PlayerUID)) return;
        
        // reflection, scary
        bool isRecipeMatched = Traverse.Create(__instance)
            .Method("MatchesRecipe")
            .GetValue<bool>();
        if (!isRecipeMatched) return;

        GetVibrationsSystem(__instance.Api)?.VibrateWithDuration(0.5f,1f);
    }
}