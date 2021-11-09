using System;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Protocol.Messages;

// spec chapter: 3.5.2
public record HubPropertyMessage(HubProperty Property, HubPropertyOperation Operation)
    : LegoWirelessMessage(MessageType.HubProperties);
public record HubPropertyMessage<TPayload>(HubProperty Property, HubPropertyOperation Operation, TPayload Payload)
    : HubPropertyMessage(Property, Operation)
{
    public override string ToString()
        => this switch
        {
            HubPropertyMessage<byte[]> msg => $"Hub Property - {msg.Property}: {BytesStringUtil.DataToString(msg.Payload)}",
            _ => $"Hub Property - {this.Property}: {this.Payload}",
        };
}

// spec chapter: 3.6.1
public record HubActionMessage(HubAction Action)
    : LegoWirelessMessage(MessageType.HubActions)
{
    public override string ToString()
        => $"Hub Action - {this.Action}";
}

// spec chapter: 3.7.1
public record HubAlertMessage(HubAlert Alert, HubAlertOperation Operation, byte DownstreamPayload = 0x00)
    : LegoWirelessMessage(MessageType.HubAlerts);

// spec chapter: 3.8.1
public abstract record HubAttachedIOMessage(byte PortId, HubAttachedIOEvent Event)
    : LegoWirelessMessage(MessageType.HubAttachedIO);

// spec chapter: 3.8.1
public record HubAttachedIOForAttachedDeviceMessage(byte PortId, DeviceType IOTypeId, Version HardwareRevision, Version SoftwareRevision)
    : HubAttachedIOMessage(PortId, HubAttachedIOEvent.AttachedIO)
{
    public override string ToString()
        => $"Attached IO - Port {HubId}/{PortId} of device type {IOTypeId} (HW: {HardwareRevision} / SW: {SoftwareRevision})";
}

// spec chapter: 3.8.1
public record HubAttachedIOForAttachedVirtualDeviceMessage(byte PortId, DeviceType IOTypeId, byte PortAId, byte PortBId)
    : HubAttachedIOMessage(PortId, HubAttachedIOEvent.AttachedVirtualIO)
{
    public override string ToString()
        => $"Attached Virtual IO - Port {HubId}/{PortId} using ports {PortAId} + {PortBId} of device type {IOTypeId}";
}

// spec chapter: 3.8.1
public record HubAttachedIOForDetachedDeviceMessage(byte PortId)
    : HubAttachedIOMessage(PortId, HubAttachedIOEvent.DetachedIO)
{
    public override string ToString()
        => $"Dettached IO - Port {HubId}{PortId}";
}

public record VirtualPortSetupMessage(VirtualPortSubCommand SubCommand)
    : LegoWirelessMessage(MessageType.VirtualPortSetup);

public record VirtualPortSetupForDisconnectedMessage(byte PortId)
    : VirtualPortSetupMessage(VirtualPortSubCommand.Disconnected);

public record VirtualPortSetupForConnectedMessage(byte PortAId, byte PortBId)
    : VirtualPortSetupMessage(VirtualPortSubCommand.Connected);
