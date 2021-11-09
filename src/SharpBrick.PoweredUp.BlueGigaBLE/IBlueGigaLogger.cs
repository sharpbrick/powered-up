using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.BlueGigaBLE;

/// <summary>
/// Just an interface to make sure all components can log their own infos and know about
/// wether they should produce the logs at all
/// </summary>
public interface IBlueGigaLogger
{
    /// <summary>
    /// returns the indented (by inserting #indent number of tabs(/t)) information about the object
    /// </summary>
    /// <param name="indent">number of tabs (/t) in the output</param>
    /// <returns></returns>
    public Task<string> GetLogInfosAsync(int indent = 0);
    /// <summary>
    /// Writes the indented information of this object into the configured ILogger
    /// </summary>
    /// <param name="indent"></param>
    public Task LogInfosAsync(int indent = 0, string header = "", string footer = "");
    /// <summary>
    /// Shall the LogInfoAsync really log
    /// </summary>
    public bool TraceDebug { get; }
}
