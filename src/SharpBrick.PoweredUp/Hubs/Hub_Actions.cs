using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp;

public abstract partial class Hub
{
    /// <summary>
    /// Switch Off Hub
    /// </summary>
    /// <returns></returns>
    public async Task SwitchOffAsync()
    {
        AssertIsConnected();

        await Protocol.SendMessageReceiveResultAsync<HubActionMessage>(new HubActionMessage(HubAction.SwitchOffHub)
        {
            HubId = HubId,
        }, action => action.Action == HubAction.HubWillSwitchOff);

        await Protocol.DisconnectAsync();
    }

    /// <summary>
    /// Disconnect Hub
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync()
    {
        AssertIsConnected();

        await Protocol.SendMessageReceiveResultAsync<HubActionMessage>(new HubActionMessage(HubAction.Disconnect)
        {
            HubId = HubId,
        }, action => action.Action == HubAction.HubWillDisconnect);

        await Protocol.DisconnectAsync();
    }

    /// <summary>
    /// VCC Port Control On
    /// </summary>
    /// <returns></returns>
    public async Task VccPortControlOnAsync()
    {
        AssertIsConnected();

        await Protocol.SendMessageAsync(new HubActionMessage(HubAction.VccPortControlOn)
        {
            HubId = HubId,
        });
    }

    /// <summary>
    /// VCC Port Control Off
    /// </summary>
    /// <returns></returns>
    public async Task VccPortControlOffAsync()
    {
        AssertIsConnected();

        await Protocol.SendMessageAsync(new HubActionMessage(HubAction.VccPortControlOff)
        {
            HubId = HubId,
        });
    }

    /// <summary>
    /// Activate BUSY Indication (Shown byRGB. Actual RGB settings preserved).
    /// </summary>
    /// <returns></returns>
    public async Task ActivateBusyIndicatorAsync()
    {
        AssertIsConnected();

        await Protocol.SendMessageAsync(new HubActionMessage(HubAction.ActivateBusyIndication)
        {
            HubId = HubId,
        });
    }

    /// <summary>
    /// Reset BUSY Indication (RGB shows the previously preserve RGB settings).
    /// </summary>
    /// <returns></returns>
    public async Task ResetBusyIndicatorAsync()
    {
        AssertIsConnected();

        await Protocol.SendMessageAsync(new HubActionMessage(HubAction.ResetBusyIndication)
        {
            HubId = HubId,
        });
    }

    protected void AssertIsConnected()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("The device needs to be connected to a protocol.");
        }
    }
}
