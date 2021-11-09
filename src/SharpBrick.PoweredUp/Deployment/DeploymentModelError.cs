namespace SharpBrick.PoweredUp.Deployment;

public class DeploymentModelError
{
    public DeploymentModelError(int errorCode, byte hubId, byte? portId, string message)
    {
        ErrorCode = errorCode;
        HubId = hubId;
        PortId = portId;
        Message = message;
    }

    public int ErrorCode { get; }
    public byte HubId { get; }
    public byte? PortId { get; }
    public string Message { get; }
}
