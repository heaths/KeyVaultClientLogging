# Logging example for KeyVaultClient

While we recommend customers upgrade to any of the following three packages with improved logging, extensibility, and performance, we understand not everyone can move right away and may need support with the older `KeyVaultClient` class from [Microsoft.Azure.KeyVault](https://www.nuget.org/packages/Microsoft.Azure.KeyVault/). This sample helps demonstrate how you can add logging to your application.

## Recommended packages

We recommend you use any of the following packages as needed. See <https://aka.ms/azsdk/intro> for reasons.

* [Azure.Security.KeyVault.Certificates](https://www.nuget.org/packages/Azure.Security.KeyVault.Certificates/)
* [Azure.Security.KeyVault.Keys](https://www.nuget.org/packages/Azure.Security.KeyVault.Keys/)
* [Azure.Security.KeyVault.Secrets](https://www.nuget.org/packages/Azure.Security.KeyVault.Secrets/)

## Logging packages

You need to add one or more of the following logging packages, or write your own class that implements `IServiceClientTracingInterceptor` from [Microsoft.Rest.ClientRuntime](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime/):

* [Microsoft.Rest.ClientRuntime.Etw](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Etw/) ([README](https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/mgmtcommon/ClientRuntime.Etw/README.md))
* [Microsoft.Rest.ClientRuntime.Log4Net](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime.Log4Net/) ([README](https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/mgmtcommon/ClientRuntime.Log4Net/README.md))

The sample code in this project uses `Microsoft.Rest.ClientRuntime.Etw`, which has almost no overhead if no ETW listeners are configured, which you can do with tools like *logman.exe* (installed with Windows), [perfview.exe](https://github.com/microsoft/perfview/), and more.

## Example

### PerfView

The easiest way to collect traces for any process on your system is to download [PerfView](https://github.com/microsoft/perfview/):

1. Run *perfview.exe*.
1. Click **Collect** -> **Collect**. If prompted to elevate, do so since tracing requires elevated privileges.
1. Expand **Advanced Options**.
1. In **Additional Providers:**, enter exactly: `*Microsoft.Rest`
1. Click **Start Collection**.
1. After running your application such as this sample application, click **Stop Collection**.
1. After processing completes (may take a while), double click **Events**.
1. Scroll down events starting with "Microsoft.Rest".

### dotnet-trace

Another useful and cross-platform tool is dotnet-trace:

1. Run: `dotnet tool install -g dotnet-trace`
1. Run this sample: `dotnet run`
1. The sample application will print the process ID and pause for you to hit **Enter**.
1. Run the following, replacing `<Process ID>` with the process ID printed from above: `dotnet trace collect --providers Microsoft.Rest -p <Process ID>`
1. Tracing will automatically stop and write the events to *trace.nettrace* in the current directory by default.

You can open *trace.nettrace* with PerfView. As above, find the events starting with "Microsoft.Rest".
