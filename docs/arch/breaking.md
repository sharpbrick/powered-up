# What is not a breaking change

- Parameter Names (paramete type is a breaking change) to existing method
- Changes to the internal elements of a type (serialization is a non-goal, reflection on internal types is a non goal)
- Changes to types outside of the supported API surface

# Supported Namespaces

- `SharpBrick.PoweredUp` (all devices, all hubs, PoweredUpHost, ILegoWirelessProtocol)
- `SharpBrick.PoweredUp.Deployment` (builder and model verifer)

# Internal Namespaces

Can be external utilized but are not considered for breaking changes

- Everything regards messages, message encoding, protocol knowledge
- Bluetooth Abstraction
- Not explicit mentioned functions
- Utils
- Anything internal to CLI