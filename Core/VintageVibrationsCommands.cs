using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageVibrations.Core;

public class VintageVibrationsCommands : ModSystem
{
    private ICoreClientAPI capi;

    private VintageVibrationsModSystem bpClient;

    public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Client;
    

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
        
        bpClient = api.ModLoader.GetModSystem<VintageVibrationsModSystem>();

        capi.ChatCommands
            .Create("vintagevibrations")
            .WithDescription("commands for vintage vibrations")

            .BeginSubCommand("up")
            .WithDescription("startup intiface server")
            .HandleWith(OnConnectVibratables)
            .EndSubCommand()

            .BeginSubCommand("down")
            .WithDescription("shutdown intiface server")
            .HandleWith(OnDisconnectVibratables)
            .EndSubCommand()

            .BeginSubCommand("vibrate")
            .WithDescription("send a simple one shot vibration signal")
            .HandleWith(OnVibrate)
            .EndSubCommand();
    }
    
    private TextCommandResult OnConnectVibratables(TextCommandCallingArgs args)
    {
        if (bpClient.IsConnected()) return TextCommandResult.Error("intiface server already connected.");
        
        bpClient.ConnectDevices();
        return TextCommandResult.Success("intiface server sucessfully connected.");
    }
    
    private TextCommandResult OnDisconnectVibratables(TextCommandCallingArgs args)
    {
        if (!bpClient.IsConnected()) return TextCommandResult.Error("no connection available.");
        
        bpClient.Shutdown();
        return TextCommandResult.Success("intiface server was shutdown.");

    }
    
    private TextCommandResult OnVibrate(TextCommandCallingArgs args)
    {
        if (!bpClient.IsConnected()) return TextCommandResult.Error("no connection available.");

        bpClient.VibrateWithDuration(0.5f, 1f);
        return TextCommandResult.Success("send vibration signal.");
    }
}