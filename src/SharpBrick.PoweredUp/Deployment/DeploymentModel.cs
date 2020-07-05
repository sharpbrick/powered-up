using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentModel
    {
        public DeploymentHubModel[] Hubs { get; }

        public DeploymentModel(DeploymentHubModel[] hubs)
        {
            if (hubs.Length > 1)
            {
                throw new NotSupportedException("Deployment verification currently only supports one Hub");
            }

            Hubs = hubs;
        }

        public Task<DeploymentModelError[]> VerifyAsync(params IPoweredUpProtocol[] protocols)
        {
            //TODO: match best hub with best protocol

            return VerifyAsync(protocols[0], 0x00, Hubs[0]);
        }

        public Task<DeploymentModelError[]> VerifyAsync(IPoweredUpProtocol protocol, byte hubId, DeploymentHubModel hubModel)
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

            if (hubModel.HubType != null && hubModel.HubType != hubInfo.SystemType)
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
                    (var Device, var PortInfo) when Device.DeviceType != null && PortInfo.IOTypeId != Device.DeviceType => (Error: 1001, t.Device, t.PortInfo), // wrong device connected
                    _ => (Error: 0, t.Device, t.PortInfo),
                })
                .Where(t => t.Error != 0)
                .Select(t => new DeploymentModelError(
                    t.Error,
                    0,
                    t.Device.PortId,
                    t.Error switch
                    {
                        1000 when t.Device.DeviceType == null => $"No device connected to port {t.Device.PortId}. Expected some device.",
                        1000 => $"No device connected to port {t.Device.PortId}. Expected {t.Device.DeviceType} on port {t.Device.PortId}.",
                        1001 => $"No {t.Device.DeviceType} connected to port {t.Device.PortId}. Currently connected: {t.PortInfo.IOTypeId}.",
                        _ => "A deployment model verificaiton failed."
                    }
                )));

            return Task.FromResult(result.ToArray());
        }
    }
}