using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    /// <summary>
    /// Just an interface to make sure all components can log their own infos and know about the
    /// if the should produce the logs at all
    /// </summary>
    public interface IBlueGigaLogger
    {
        /// <summary>
        /// returns the indented (by inserting #indent number of tabs(/t)) information about the object
        /// </summary>
        /// <param name="indent"></param>
        /// <returns></returns>
        public String GetMyLogInfos(int indent = 0);
        /// <summary>
        /// Writes the indented information of this object into the configured ILogger
        /// </summary>
        /// <param name="indent"></param>
        public void LogMyInfos(int indent = 0, String header="" , String footer="");
        /// <summary>
        /// Shall the LogMyInfos really log
        /// </summary>
        public bool TraceDebug { get; set; }
    }
}
