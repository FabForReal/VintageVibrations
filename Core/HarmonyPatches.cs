using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

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
    
    [HarmonyPatch(typeof(EntityPlayer), "OnHurt")]
    [HarmonyPostfix]
    private static void OnHurt(DamageSource damageSource, float damage, EntityPlayer __instance)
    {
        if (__instance.Api.Side != EnumAppSide.Client) return;
        
        const float minDamage = 0.5f;
        const float maxDamage = 21.0f;
        
        damage = Math.Clamp(damage, minDamage, maxDamage);
        float f = 0.1f + ((damage - minDamage) / (maxDamage - minDamage)) * 0.9f;
        
        GetVibrationsSystem(__instance.Api)?.VibrateWithDuration(f,0.5f);
    }
}