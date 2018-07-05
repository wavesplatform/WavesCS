# WavesCS
A C# library for interacting with the Waves blockchain

Supports node interaction, offline transaction signing, Matcher orders, and creating addresses and keys.

## Getting Started

You can install **WavesPlatform.WavesCS** [NuGet package](https://www.nuget.org/packages/WavesPlatform.WavesCS/) and add it to your project's References and in your code as:
```
using WavesCS;
```

For installation NuGet package from VS Package Manager Console you should use:
```
PM> Install-Package WavesPlatform.WavesCS -Version 1.1.0
```

For installation via UI Package Manager use this [instruction](https://docs.microsoft.com/en-us/nuget/tools/package-manager-ui).

Target framework .NET Framework 4.5.1
## Documentation

The library utilizes classes to represent various Waves data structures and encoding and serialization methods:

- WavesCS.Node
- WavesCS.Order
- WavesCS.OrderBook
- WavesCS.PrivateKeyAccount
- WavesCS.Transaction
- WavesCS.AddressEncoding
- WavesCS.Base58
- WavesCS.Utils


#### Code Example
Code examples are in [WavesCSTests](https://github.com/wavesplatform/WavesCS/tree/master/WavesCSTests) project.

### Source code
[WavesCS Github repository](https://github.com/wavesplatform/WavesCS)

### Topic on Waves Forum
https://forum.wavesplatform.com/t/wavescs-c-client-library-for-waves-api/83
