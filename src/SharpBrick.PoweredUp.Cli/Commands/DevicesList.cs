using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Cli;

public class DevicesList
{
    private readonly ILegoWirelessProtocol protocol;
    private readonly DiscoverPorts discoverPorts;

    public DevicesList(ILegoWirelessProtocol protocol, DiscoverPorts discoverPorts)
    {
        this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        this.discoverPorts = discoverPorts ?? throw new ArgumentNullException(nameof(discoverPorts));
    }
    public async Task ExecuteAsync(SystemType knownSystemType)
    {
        Console.WriteLine("Discover Ports. Receiving Messages ...");

        await protocol.ConnectAsync(knownSystemType); // registering to bluetooth notification

        await Task.Delay(2000); // await ports to be announced initially by device.

        using var disposable = protocol.UpstreamMessages.Subscribe(x => Console.Write("."));

        await discoverPorts.ExecuteAsync();

        await protocol.SendMessageReceiveResultAsync<HubActionMessage>(new HubActionMessage(HubAction.SwitchOffHub) { HubId = 0 }, result => result.Action == HubAction.HubWillSwitchOff);

        Console.WriteLine(string.Empty);

        Console.WriteLine($"Discover Ports Function: {discoverPorts.ReceivedMessages} / {discoverPorts.SentMessages}");

        PrettyPrintKnowledge(System.Console.Out, protocol.Knowledge, false);
    }

    public static void PrettyPrintKnowledge(TextWriter writer, ProtocolKnowledge portKnowledge, bool showConfiguration = true)
    {
        string Intent(int depth) => "                        ".Substring(0, depth * 2);

        string MinMaxDescription(float rawMin, float rawMax, float min, float max)
        {
            bool isPassThrough = (rawMax == max) && (rawMin == min);
            bool isTranslation = (rawMax - rawMin) == (max - min);
            bool isScaling = !((rawMax == max) && (rawMin == min));

            return (isPassThrough, isTranslation, isScaling) switch
            {
                (true, _, _) => " (pass-through)",
                (false, false, false) => string.Empty,
                (false, false, true) => " (scaling)",
                (false, true, false) => " (translation)",
                (false, true, true) => " (scaling, translation)",
            };
        }

        foreach (var hub in portKnowledge.Hubs.OrderBy(h => h.HubId))
        {
            Console.WriteLine($"{Intent(0)}- Hub: 0x{hub.HubId:X2} / {hub.HubId}");
            foreach (var port in hub.Ports.Values.OrderBy(p => p.PortId))
            {
                writer.WriteLine($"{Intent(1)}- Port: 0x{port.PortId:X2} / {port.PortId}");

                writer.WriteLine($"{Intent(2)}- IOTypeId: {port.IOTypeId} / 0x{(ushort)port.IOTypeId:X4} / {(ushort)port.IOTypeId}");
                writer.WriteLine($"{Intent(3)}Revision: SW: {port.SoftwareRevision}, HW: {port.HardwareRevision}");
                writer.Write($"{Intent(3)}Capabilities:");
                if (port.OutputCapability)
                {
                    writer.Write(" Output");
                }
                if (port.InputCapability)
                {
                    writer.Write(" Input");
                }
                if (port.LogicalCombinableCapability)
                {
                    writer.Write(" LogicalCombinable");
                }
                if (port.LogicalSynchronizableCapability)
                {
                    writer.Write(" LogicalSynchronizable");
                }
                writer.WriteLine(string.Empty);

                // PortInformationForPossibleModeCombinationsMessage
                writer.WriteLine($"{Intent(3)}ModeCombinations: [{string.Join(", ", port.ModeCombinations.Select(x => BytesStringUtil.ToBitString(x)))}]");

                if (showConfiguration)
                {
                    writer.WriteLine($"{Intent(2)}- Configuration");
                    writer.WriteLine($"{Intent(3)}UsedCombinationIndex: {port.UsedCombinationIndex}");
                    writer.WriteLine($"{Intent(3)}MultiUpdateEnabled: {port.MultiUpdateEnabled}");
                    writer.WriteLine($"{Intent(3)}ConfiguredModeDataSetIndex: [{string.Join(",", port.ConfiguredModeDataSetIndex)}]");
                }

                foreach (var mode in port.Modes.Values.OrderBy(m => m.ModeIndex))
                {
                    writer.Write($"{Intent(2)}- Mode {mode.ModeIndex}: Name: {mode.Name}, Symbol: {mode.Symbol}, Capability:");
                    writer.Write(mode.IsInput ? " Input" : string.Empty);
                    writer.WriteLine(mode.IsOutput ? " Output" : string.Empty);

                    writer.WriteLine($"{Intent(3)}- DataSet: {mode.NumberOfDatasets}x {mode.DatasetType}, TotalFigures: {mode.TotalFigures}, Decimals: {mode.Decimals}");

                    if (mode.IsInput || (!mode.IsInput && (mode.InputSupportsNull || mode.InputSupportFunctionalMapping20 || mode.InputAbsolute || mode.InputDiscrete || mode.InputRelative)))
                    {
                        writer.Write($"{Intent(4)}Input Mapping:");
                        if (mode.InputSupportsNull)
                        {
                            writer.Write(" SupportsNull");
                        }
                        if (mode.InputSupportFunctionalMapping20)
                        {
                            writer.Write(" SupportFunctionalMapping20");
                        }
                        if (mode.InputAbsolute)
                        {
                            writer.Write(" Absolute");
                        }
                        if (mode.InputRelative)
                        {
                            writer.Write(" Relative");
                        }
                        if (mode.InputDiscrete)
                        {
                            writer.Write(" Discrete");
                        }
                        writer.WriteLine();
                    }

                    if (mode.IsOutput || (!mode.IsOutput && (mode.OutputSupportsNull || mode.OutputSupportFunctionalMapping20 || mode.OutputAbsolute || mode.OutputDiscrete || mode.OutputRelative)))
                    {
                        writer.Write($"{Intent(4)}Output Mapping:");
                        if (mode.OutputSupportsNull)
                        {
                            writer.Write(" SupportsNull");
                        }
                        if (mode.OutputSupportFunctionalMapping20)
                        {
                            writer.Write(" SupportFunctionalMapping20");
                        }
                        if (mode.OutputAbsolute)
                        {
                            writer.Write(" Absolute");
                        }
                        if (mode.OutputRelative)
                        {
                            writer.Write(" Relative");
                        }
                        if (mode.OutputDiscrete)
                        {
                            writer.Write(" Discrete");
                        }
                        writer.WriteLine();
                    }

                    writer.WriteLine($"{Intent(4)}Raw Min: {mode.RawMin,7}, Max: {mode.RawMax,7}");
                    writer.WriteLine($"{Intent(4)}Pct Min: {mode.PctMin,7}, Max: {mode.PctMax,7}{MinMaxDescription(mode.RawMin, mode.RawMax, mode.PctMin, mode.PctMax)}");
                    writer.WriteLine($"{Intent(4)}SI  Min: {mode.SIMin,7}, Max: {mode.SIMax,7}{MinMaxDescription(mode.RawMin, mode.RawMax, mode.SIMin, mode.SIMax)}");

                    if (showConfiguration)
                    {
                        writer.WriteLine($"{Intent(3)}- Configuration");
                        writer.WriteLine($"{Intent(4)}NotificationEnabled: {mode.NotificationEnabled}");
                        writer.WriteLine($"{Intent(4)}DeltaInterval: {mode.DeltaInterval}");
                    }
                }
            }
        }
    }
}
