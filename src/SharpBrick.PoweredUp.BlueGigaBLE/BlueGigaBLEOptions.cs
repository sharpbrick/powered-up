namespace SharpBrick.PoweredUp;

public class BlueGigaBLEOptions
{
    public BlueGigaBLEOptions()
    {
    }

    public BlueGigaBLEOptions(string comPortName, bool traceDebug)
    {
        COMPortName = comPortName;
        TraceDebug = traceDebug;
    }

    /// <summary>
    /// The name of the COM-Port to use (for example "COM4"  under Windows. You've got to look up under what COM-port your BlueGiga-adapter works...
    /// </summary>
    public string COMPortName { get; set; } = "COM4";
    /// <summary>
    /// Show debug-trace in the BlueGigaBLE-Implementation (is produces a lot!)
    /// It will log to a configured ILogger;so if this is geiven/configured, the default is to produce the Log-entries
    /// </summary>
    public bool TraceDebug { get; set; } = true;
}
