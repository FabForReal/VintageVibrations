using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageVibrations.Core;

public class VintageVibrationsEvents : ModSystem
{
    private ICoreClientAPI capi;

    private VintageVibrationsModSystem? bpClient;
    
    public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Client;
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
        bpClient = api.ModLoader.GetModSystem<VintageVibrationsModSystem>();
        
        capi.Event.PlayerEntitySpawn += EventOnPlayerEntitySpawn;
    }

    private void EventOnPlayerEntitySpawn(IClientPlayer byPlayer)
    {
        if (byPlayer == capi.World.Player)
        {
            capi.ShowChatMessage("test");
        }
    }

    public override void Dispose()
    {
        capi.Event.PlayerEntitySpawn -= EventOnPlayerEntitySpawn;
    }
}