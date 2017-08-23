# FINT.Sse client .NET

Built on [EventSource4Net](https://github.com/erizet/EventSource4Net).

## Installation

Configure your package source to include "http://dl.bintray.com/fint/nuget" by adding the following to your project nuget.config file
[See the MSDN documentation](https://docs.microsoft.com/en-us/nuget/schema/nuget-config-file)

```nuget.config
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
	<add key="Bintray" value="http://dl.bintray.com/fint/nuget" />
  </packageSources>
</configuration>

Install-Package Fint.Sse
```

## Usage

It's dead-simple to use.

```csharp
EventSource es = new EventSource(new Uri(<Your url>));
es.StateChanged += new EventHandler<StateChangedEventArgs>((o, e) => { Console.WriteLine("New state: " + e.State.ToString()); });
es.EventReceived += new EventHandler<ServerSentEventReceivedEventArgs>((o, e) => { Console.WriteLine("--------- Msg received -----------\n" + e.Message.ToString()); });
es.Start();
```

See the sample-project!

## Configuration
TBD

## OAuth
TBD

**Basic authentication**  
TBD

**[OAuth config](https://github.com/FINTlibs/fint-oauth-token-service#configuration)**
