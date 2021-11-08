using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp.Deployment;

public class DeploymentModel
{
    /// <summary>
    /// Expected Hubs in Model
    /// </summary>
    /// <value></value>
    public DeploymentHubModel[] Hubs { get; }

    public DeploymentModel(DeploymentHubModel[] hubs)
    {
        if (hubs.Length > 1)
        {
            throw new NotSupportedException("Deployment verification currently only supports one Hub");
        }

        Hubs = hubs;
    }

    /// <summary>
    /// Verify the model against a list of protocols (e.g. due to multiple BT connections)
    /// </summary>
    /// <param name="protocols"></param>
    /// <returns></returns>
    public DeploymentModelError[] Verify(params ILegoWirelessProtocol[] protocols)
    {
        //TODO: match best hub with best protocol

        return Verify(protocols[0], 0x00, Hubs[0]);
    }

    /// <summary>
    /// Verify a given Hub (protocol + hubId) aganinst a HubModel
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="hubId"></param>
    /// <param name="hubModel"></param>
    /// <returns></returns>
    public DeploymentModelError[] Verify(ILegoWirelessProtocol protocol, byte hubId, DeploymentHubModel hubModel)
    {
        if (protocol is null)
        {
            throw new ArgumentNullException(nameof(protocol));
        }

        if (hubModel is null)
        {
            throw new ArgumentNullException(nameof(hubModel));
        }

        var result = new List<DeploymentModelError>();

        var hubInfo = protocol.Knowledge.Hub(hubId);

        if (hubModel.HubType is not null && hubModel.HubType != hubInfo.SystemType)
        {
            result.Add(new DeploymentModelError(1002, hubInfo.HubId, null, $"Hub {hubInfo.HubId} with system type {hubInfo.SystemType} does not match expected {hubModel.HubType}."));
        }

        result.AddRange(
            hubModel.Devices.Select(Device =>
            {
                var PortInfo = protocol.Knowledge.Port(hubId, Device.PortId);

                return (Device, PortInfo);
            })
            .Select(t => t switch
            {
                (var Device, var PortInfo) when !PortInfo.IsDeviceConnected => (Error: 1000, t.Device, t.PortInfo), // no device connected
                    (var Device, var PortInfo) when Device.DeviceType is not null && PortInfo.IOTypeId != Device.DeviceType => (Error: 1001, t.Device, t.PortInfo), // wrong device connected
                    _ => (Error: 0, t.Device, t.PortInfo),
            })
            .Where(t => t.Error != 0)
            .Select(t => new DeploymentModelError(
                t.Error,
                0,
                t.Device.PortId,
                t.Error switch
                {
                    1000 when t.Device.DeviceType is null => $"No device connected to port {t.Device.PortId}. Expected some device.",
                    1000 => $"No device connected to port {t.Device.PortId}. Expected {t.Device.DeviceType} on port {t.Device.PortId}.",
                    1001 => $"No {t.Device.DeviceType} connected to port {t.Device.PortId}. Currently connected: {t.PortInfo.IOTypeId}.",
                    _ => "A deployment model verificaiton failed."
                }
            )));

        return result.ToArray();
    }
}
