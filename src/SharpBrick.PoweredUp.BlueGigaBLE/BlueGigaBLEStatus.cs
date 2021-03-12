using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEStatus
    {
        public const UInt16 STATE_STANDBY = 0;
        public const UInt16 STATE_SCANNING = 1;
        public const UInt16 STATE_CONNECTING = 2;
        public const UInt16 STATE_FINDING_SERVICES = 3;
        public const UInt16 STATE_FINDING_ATTRIBUTES = 4;
        //public const UInt16 STATE_LISTENING_MEASUREMENTS = 5;
        public const UInt16 STATE_RESETING = 6;

        private UInt16 actualStatus;
        private ILogger _logger;
        private bool _traceDebug;

        public BlueGigaBLEStatus(ushort actualStatus, ILogger logger, bool traceDebug)
        {
            ActualStatus = actualStatus;
            _logger = logger;
            _traceDebug = traceDebug;
            if (_traceDebug) _logger?.LogDebug($"BlueGigaBLEStatus inititalized with status {actualStatus} {GetStatusText(actualStatus)}");
        }

        private String GetStatusText(UInt16 status)
        {
            return status switch
            {
                STATE_STANDBY => "[STAND BY]",
                STATE_SCANNING => "[SCANNING FOR DEVICES]",
                STATE_CONNECTING => "[CONNECTING TO A DEVICE ]",
                STATE_FINDING_SERVICES => "[SEARCHING FOR SERVICES/GROUPS]",
                STATE_FINDING_ATTRIBUTES => "[SEARCHING FOR ATTRIBUTE/CHARACTERISTICS]",
                STATE_RESETING => "[RESETING BLUETOOTH-ADAPTER]",
                _ => "[UNKNOWN STATE]"

            };
        }

        public UInt16 ActualStatus
        {
            get { return actualStatus; }
            set
            {
                if (_traceDebug)
                    _logger?.LogDebug($"BlueGigaBLEStatus changes status from {actualStatus} {GetStatusText(actualStatus)} to {value} {GetStatusText(value)}");
                actualStatus = value;
            }
        }

    }
}
