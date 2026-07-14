using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageVibrations.Core;

public class VintageVibrationsCommands : ModSystem
{
    private ICoreClientAPI capi;

    private VintageVibrationsModSystem vibrationSystem;

    public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Client;
    

    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
        
        vibrationSystem = api.ModLoader.GetModSystem<VintageVibrationsModSystem>();

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
            .EndSubCommand()
        
            .BeginSubCommand("status")
            .WithDescription("intiface status")
            .HandleWith(OnStatus)
            .EndSubCommand();
    }
    
    private TextCommandResult OnConnectVibratables(TextCommandCallingArgs args)
    {
        if (vibrationSystem.IsConnected()) return TextCommandResult.Error("intiface server already connected.");
        
        vibrationSystem.ConnectDevices();
        return TextCommandResult.Success("trying to connect to intiface server...");
    }
    
    private TextCommandResult OnDisconnectVibratables(TextCommandCallingArgs args)
    {
        if (!vibrationSystem.IsConnected()) return TextCommandResult.Error("no connection available.");
        
        vibrationSystem.Shutdown();
        return TextCommandResult.Success("intiface server was shutdown.");

    }
    
    private TextCommandResult OnVibrate(TextCommandCallingArgs args)
    {
        if (!vibrationSystem.IsConnected()) return TextCommandResult.Error("no connection available.");

        vibrationSystem.VibrateWithDuration(0.5f, 1f);
        return TextCommandResult.Success("send vibration signal.");
    }
    
    private TextCommandResult OnStatus(TextCommandCallingArgs args)
    {
        if (!vibrationSystem.IsConnected()) return TextCommandResult.Error("no connection available.");
        return TextCommandResult.Success("intiface sever connected.");
    }
}