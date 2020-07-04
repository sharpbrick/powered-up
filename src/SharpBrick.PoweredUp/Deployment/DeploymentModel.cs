using System;
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

            return VerifyAsync(protocols[0], Hubs[0]);
        }

        public Task<DeploymentModelError[]> VerifyAsync(IPoweredUpProtocol protocol, DeploymentHubModel hubModel)
        {
            if (protocol is null)
            {
                throw new ArgumentNullException(nameof(protocol));
            }

            if (hubModel is null)
            {
                throw new ArgumentNullException(nameof(hubModel));
            }

            // TODO: check hub type

            var result = hubModel.Devices.Select(Device =>
            {
                var PortInfo = protocol.Knowledge.Port(Device.PortId);

                return (Device, PortInfo);
            })
            .Select(t => t switch
            {
                (var Device, var PortInfo) when !PortInfo.IsDeviceConnected => (Error: 1000, t.Device, t.PortInfo), // no device connected
                (var Device, var PortInfo) when PortInfo.IOTypeId != Device.DeviceType => (Error: 1001, t.Device, t.PortInfo), // wrong device connected
                _ => (Error: 0, t.Device, t.PortInfo),
            })
            .Where(t => t.Error != 0)
            .Select(t => new DeploymentModelError(
                t.Error,
                0,
                t.Device.PortId,
                t.Error switch
                {
                    1000 => $"No device connected to port {t.Device.PortId}. Expected {t.Device.DeviceType} on port {t.Device.PortId}.",
                    1001 => $"No {t.Device.DeviceType} connected to port {t.Device.PortId}. Currently connected: {t.PortInfo.IOTypeId}.",
                    _ => "A deployment model verificaiton failed."
                }
            ))
            .ToArray();

            return Task.FromResult(result);
        }
    }
}