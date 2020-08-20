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

### Code changes

To enable logging, you have to add trace listeners to your code and enable logging:

```csharp
ServiceClientTracing.AddTracingInterceptor(new EtwTracingInterceptor());
ServiceClientTracing.IsEnabled = true;
```

In contrast, using the new Azure.\* packages like those recommended above enables secure logging by default. That is, no personally-identifiable information (PII) is logged. You can *skip* making code changes and listen for `Azure` events, like `Azure-Core` for pipeline requests and responses. Read [Tracing Azure SDK for .NET](https://heaths.dev/azure/2020/02/04/trace-azure-sdk-for-net.html) for more information.

## Tracing events

.NET Core uses an `EventSource` to log data, which [uses ETW on Windows and LTTng on Linux](https://docs.microsoft.com/dotnet/core/diagnostics/logging-tracing#logging-events). If you are running on the .NET Framework on Windows, you can use a configuration file like the included [app.config](app.config), though this has no effect on .NET Core.

Note that logging data from `HttpClient` will likely log personally identifiable information (PII) including secret information like bearer tokens. In our newer Azure.\* packages, we not only obfuscate such information (only some headers are logged, while all others are sanitized) but we do not log content by default, since content will likely contain PII.

### PerfView

The easiest way to collect traces for any process on your system is to download [PerfView](https://github.com/microsoft/perfview/):

1. Run *perfview.exe*.
1. Click **Collect** -> **Collect**. If prompted to elevate, do so since tracing requires elevated privileges.
1. Expand **Advanced Options**.
1. In **Additional Providers:**, enter exactly: `*Microsoft.Rest`
1. *Optional*: If you want to log all messages from `HttpClient` used internally, enter exactly: `*Microsoft-System-Net-Http,*Microsoft.Rest`
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
1. *Optional*: If you want to log all messages from `HttpClient` used internally, enter instead: `dotnet trace collect --providers Microsoft-System-Net-Http,Microsoft.Rest -p <Process ID>`
1. Tracing will automatically stop and write the events to *trace.nettrace* in the current directory by default.

You can open *trace.nettrace* with PerfView. As above, find the events starting with "Microsoft.Rest".
