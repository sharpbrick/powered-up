namespace SharpBrick.PoweredUp.Protocol.Messages;

// spec chapter: 3.15.2
public enum PortInformationType : byte
{
    PortValue = 0x00,
    ModeInfo = 0x01,
    PossibleModeCombinations = 0x02,
}
