# Add new device tutorial

1. Create new device in namespace `SharpBrick.PoweredUp` and folder `src/SharpBrick.PoweredUp/Devices`
2. Derive from `Device`
3. Create empty constructor (for non-interactive purposes) and `ctr(ILegoWirelessProtocol protocol, byte hubId, byte portId)` (for interactive connected purposes)
4. Dervice from interface `IPoweredUpDevice`
5. Execute `poweredup device dump-static-port -p <port number>` to retrieve static port information.
6. Add dump to method `IPoweredUpDevice.GetStaticPortInfoMessages` (this method improves startup performance since protocol knowledge can be initialized without lenthly querying the actual devices)
7. Wire up observable for input events
8. Wire up known commands messages.
9. Add to DeviceFactory