using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEStatus
    {
        public const ushort StateStandby = 0;
        public const ushort StateScanning = 1;
        public const ushort StateConnecting = 2;
        public const ushort StateFindingServices = 3;
        public const ushort StateFindingattributes = 4;
        public const ushort StateReseting = 5;

        private ushort actualStatus;
        private readonly ILogger logger;
        private readonly bool _traceDebug;

        public BlueGigaBLEStatus(ushort actualStatus, ILogger logger, bool traceDebug)
        {
            ActualStatus = actualStatus;
            this.logger = logger;
            _traceDebug = traceDebug;
            if (_traceDebug)
            {
                this.logger?.LogDebug($"BlueGigaBLEStatus inititalized with status {actualStatus} {GetStatusText(actualStatus)}");
            }
        }

        private string GetStatusText(ushort status)
        {
            return status switch
            {
                StateStandby => "[STAND BY]",
                StateScanning => "[SCANNING FOR DEVICES]",
                StateConnecting => "[CONNECTING TO A DEVICE ]",
                StateFindingServices => "[SEARCHING FOR SERVICES/GROUPS]",
                StateFindingattributes => "[SEARCHING FOR ATTRIBUTE/CHARACTERISTICS]",
                StateReseting => "[RESETING BLUETOOTH-ADAPTER]",
                _ => "[UNKNOWN STATE]"

            };
        }

        public ushort ActualStatus
        {
            get => actualStatus;
            set
            {
                if (_traceDebug)
                {
                    logger?.LogDebug($"BlueGigaBLEStatus changes status from {actualStatus} {GetStatusText(actualStatus)} to {value} {GetStatusText(value)}");
                }
                actualStatus = value;
            }
        }

    }
}
