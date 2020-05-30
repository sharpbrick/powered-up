# SharpBrick.PoweredUp

SharpBrick.PoweredUp is a .NET implementation of the BlueTooth Low Energy Protocol for Lego Powered Up products. [Specification](https://lego.github.io/lego-ble-wireless-protocol-docs/) and the [node-poweredup](https://github.com/nathankellenicki/node-poweredup) protocol implementation.

![Build-Release](https://github.com/sharpbrick/powered-up/workflows/Build-Release/badge.svg)
![Build-CI](https://github.com/sharpbrick/powered-up/workflows/Build-CI/badge.svg)
![license:MIT](https://img.shields.io/github/license/sharpbrick/powered-up?style=flat-square)
[![Nuget](https://img.shields.io/nuget/v/SharpBrick.PoweredUp?style=flat-square)](https://www.nuget.org/packages/SharpBrick.PoweredUp/)



## Implementation Status

- Bluetooth Adapter
  - [X] .NET Core 3.1 (on Windows 10 using WinRT)
    - Library uses `Span<T>` therefore not supported in WinRT until arrival of .NET 5.
    - Library uses WinRT for communication therefore only Windows 10
  - [ ] Xamarin.Essentials
  - [ ] Blazor / WebBluetooth
- Hub Model
  - Hubs
    - [ ] Technic Medium Hub
    - .. other hubs depend on availability of hardware / contributions
  - Devices
    - [ ] Technic XLarge Motor
    - [ ] Technic Large Motor
- Message Encoder
  - [X] [3.1 Common Message Header](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#common-message-header)
  - [X] [3.2. Message Length Encoding](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#message-length-encoding)
  - [X] [3.3. Message Types (Enum)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#message-types)
  - [ ] [3.4 Transmission Flow Control (Preliminary PROPOSAL!)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#transmission-flow-control-preliminary-proposal)
  - [X] [3.5. Hub Properties](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-properties)
  - [X] [3.6. Hub Actions](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-actions)
  - [X] [3.7. Hub Alerts](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-alerts)
  - [X] [3.8. Hub Attached I/O](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-attached-i-o)
  - [X] [3.9. Generic Error Messages](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#generic-error-messages)
  - ~~[ ] [3.10. H/W NetWork Commands](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#h-w-network-commands)~~
  - ~~[ ] [3.11. F/W Update - Go Into Boot Mode](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-update-go-into-boot-mode)~~
  - ~~[ ] [3.12. F/W Update Lock memory](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-update-lock-memory)~~
  - ~~[ ] [3.13. F/W Update Lock Status Request](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-update-lock-status-request)~~
  - ~~[ ] [3.14. F/W Lock Status](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-lock-status)~~
  - [X] [3.15. Port Information Request](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-information-request)
  - [X] [3.16. Port Mode Information Request](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-mode-information-request)
  - [ ] [3.17. Port Input Format Setup (Single)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-setup-single)
  - [ ] [3.18. Port Input Format Setup (CombinedMode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-setup-combinedmode)
  - [X] [3.19. Port Information](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-information)
  - [X] [3.20. Port Mode Information](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-mode-information)
  - [ ] [3.21. Port Value (Single)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-value-single)
  - [ ] [3.22. Port Value (CombinedMode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-value-combinedmode)
  - [ ] [3.23. Port Input Format (Single)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-single)
  - [ ] [3.24. Port Input Format (CombinedMode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-combinedmode)
  - [ ] [3.25. Virtual Port Setup](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#virtual-port-setup)
  - [ ] [3.26. Port Output Command](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-output-command)
  - [ ] [3.27. Output Command 0x81 - Motor Sub Commands [0x01-0x3F]](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-command-0x81-motor-sub-commands-0x01-0x3f)
  - [ ] [3.28. WriteDirect](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#writedirect)
  - [ ] [3.29. WriteDirectModeData](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#writedirectmodedata)
  - [ ] [3.30. Checksum Calculation](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#checksum-calculation)
  - [ ] [3.31. Tacho Math](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#tacho-math)
  - [ ] [3.32. Port Output Command Feedback](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-output-command-feedback)



## Other Languages

- JavaScript (Node + Browser): 
  - [node-poweredup](https://github.com/nathankellenicki/node-poweredup)

## Contribution

SharpBrick is an organization intended to host volunteers willing to contribute to the SharpBrick.PoweredUp and related projects. Everyone is welcome (private and commercial entities), [Code of Conduct](CODE_OF_CONDUCT.md) included.

The product is licensed under **MIT License** to allow a easy and wide adoption into prviate and commercial products.
