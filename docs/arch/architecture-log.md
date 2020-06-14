## 2020-06-14 Creating a single namespace for popular classes

- Intention: Consumers should not need to search namespaces to discover an API
- Solution:
  - Break namespace / folder alignment (e.g. sub folder Devices/Hubs despite all being in root namespace)
  - Move re-used types (mainly enums from Messages / Devices into the root namespace with separate folder Enums)
- Rationale: Consumer usability is more important than code organization

## 2020-06-01 Port(Combined)Value encoding and consequences

- PortValue and PortCombinedValue are encoding their payload data according to definitions set-up in PortInputFormat(Combined)Setup requests. As a result, the protocol decoder itself is stateful.
  - Port Knowledge has to be present already on the Protocol Level and not the Hub/Device Logic Models
  - The Protocol will need middlewares which can maintain the knowledge, irrelevant of the uses of the protocol
  - Since querying all many properties (PortInformation, PortModeInformation) take significant time and their properties are static by device type, hardcoded caching by device type/fw/hw is possible. An explicit discovery mode for unknown devices can be implemented as a "business function".
  - The remaining informaton (HubAttachedIO Attach/Detach, PortInputFormat) are user choices (in physical doing or software), their input is dynamic and needs implementation as middlewares.

Involved Messages

- `HubAttachedIOMessage` (event Attached): existence and device type.
- `HubAttachedIOMessage` (event Detached): removal of existence, port knowledge reset.
- `PortInputFormatSingleMessage`: format description of the `PortValueSingleMessage`.
- `PortInputFormatCombinedModeMessage`: format description of the `PortValueCombinedModeMessage`.

- `PortInformationMessage` (mode info + mode combinations): basic device description (cachable).
- `PortModeInformationMessage` (~10 properties): device property description (cachable).

Architecture Changes:

- Establishment of a Knowledge Model `SharpBrick.PoweredUp.Knowledge`.
- No introduction of a middleware stack in  `PoweredUPProtocol`. Not yet needed.
- Protocol function to lookup up knowledge.
- Protocol function to update knowledge.
- Business Function `DiscoverPorts` which acts on top of the `PoweredUpProtocol` but can access the knowledge