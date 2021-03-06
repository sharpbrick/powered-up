namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.7.1
    public record HubAlertMessage(HubAlert Alert, HubAlertOperation Operation, byte DownstreamPayload = 0x00) : LegoWirelessMessage(MessageType.HubAlerts);
}