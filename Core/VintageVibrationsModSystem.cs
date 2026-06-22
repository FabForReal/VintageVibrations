using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buttplug.Client;
using Buttplug.Core;
using Buttplug.Core.Messages;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: ModInfo("VintageVibrations", "vintagevibrations",
                    Authors = ["fabforreal"],
                    Description = "buttplug.io support for vintage story",
                    Version = "1.0.0", 
                    Side = "Client", RequiredOnClient = true)]

namespace VintageVibrations.Core;

public class VintageVibrationsModSystem : ModSystem
{
    private ICoreClientAPI capi;

    private ButtplugClient? bpClient;
    
    private const string DefaultUri = "ws://localhost:12345";
    private static readonly string ServerUri = DefaultUri;
    private static List<ButtplugClientDevice> ConnectedDevices;

    
    public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Client;
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        capi = api;
        bpClient = new ButtplugClient("VintageVibrations");
        
        ConnectedDevices = [];
        bpClient.DeviceAdded += OnDeviceAdded;
        bpClient.DeviceRemoved += OnDeviceRemoved;
    }
    
    public void Shutdown()
    {
        if(!IsConnected()) return;
        
        try
        {
            Clear();
        }
        catch (Exception e)
        {
            capi.Logger.Error($"error occured while shutting down: {e}");
        }
    }

    public async void ConnectDevices()
    {
        if (IsConnected()) return;

        try
        {
            capi.Logger.Notification($"Attempting to connect to Intiface server at {ServerUri}");
            await bpClient.ConnectAsync(new ButtplugWebsocketConnector(new Uri(ServerUri)));
            capi.Logger.Notification("Connection successful. Beginning scan for devices");
            await bpClient.StartScanningAsync();
        }
        catch (ButtplugException e)
        {
            capi.Logger.Error($"buttplug.io error occured while connecting devices: {e}");
        }
    }

    /// <summary>
    ///     start a vibration with a duration
    /// </summary>
    /// <param name="intensity"></param>
    /// <param name="duration"></param>
    public void VibrateWithDuration(float intensity, float duration)
    {
        if (!IsConnected()) return;
        
        ConnectedDevices.ForEach(Action);
        return;

        async void Action(ButtplugClientDevice device)
        {
            try
            {
                await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(Math.Clamp(intensity, 0f, 1.0f)));
                await Task.Delay((int)(duration * 1000f));
                await device.StopAsync();
            }
            catch (Exception e)
            {
                capi.Logger.Error($"Failed to send vibration to device: {e.Message}");
            }
        }
    }

    /// <summary>
    ///     start a continuous vibration
    /// </summary>
    /// <param name="intensity"></param>
    public void Vibrate(float intensity)
    {
        if (!IsConnected()) return;

        ConnectedDevices.ForEach(Action);
        return;

        async void Action(ButtplugClientDevice device)
        {
            try
            {
                await device.RunOutputAsync(DeviceOutput.Vibrate.Percent(Math.Clamp(intensity, 0f, 1.0f)));
            }
            catch (Exception e)
            {
                capi.Logger.Error($"Failed to send vibration to device: {e.Message}");
            }
        }
    }

    /// <summary>
    ///     stops the vibrations for all connected devices
    /// </summary>
    public void StopAllDevices()
    {
        if(!IsConnected()) return;
        
        ConnectedDevices.ForEach(async void (device) =>
        {
            try
            {
                await device.StopAsync();
            }
            catch (Exception e)
            {
                capi.Logger.Error($"Failed to send stop signal to device: {e.Message}");
            }
        });
    }
    
    public bool IsVibratable(ButtplugClientDevice device)
    {
        return device.HasOutput(OutputType.Vibrate);
    }
    
    public bool IsConnected()
    {
        if (bpClient == null) 
            return false;
        return bpClient.Connected;
    }

    private void OnDeviceAdded(object? sender, DeviceAddedEventArgs e)
    {
        if (!IsVibratable(e.Device)) return;

        ConnectedDevices.Add(e.Device);
        
        capi.Logger.Notification($"{e.Device.Name} connected to client {bpClient.Name}");
    }

    private void OnDeviceRemoved(object? sender, DeviceRemovedEventArgs e)
    {
        if (!IsVibratable(e.Device)) return;

        ConnectedDevices.Remove(e.Device);
        capi.Logger.Notification($"{e.Device.Name} disconnected from client {bpClient.Name}");
    }

    private void Clear()
    {
        if(bpClient == null) return;
        bpClient.StopScanningAsync();
        bpClient.DisconnectAsync();
        ConnectedDevices.Clear();
    }

    public override void Dispose()
    {
        Clear();
        bpClient?.Dispose();
    }
}