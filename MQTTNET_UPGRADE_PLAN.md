# MQTTnet v5 Upgrade Plan

## Background

The project currently depends on `MQTTnet` and `MQTTnet.Extensions.ManagedClient` at version
**4.3.6.1152**. Upgrading to the latest stable release (v5.x) is blocked because the managed
client extension was removed in v5 as a breaking change. The managed client provided three
capabilities that the current code relies on:

1. **Automatic reconnection** — the client reconnects without manual intervention after a
   connection drop.
2. **Message queue / `EnqueueAsync`** — messages are accepted while disconnected and are sent
   automatically once the connection is restored.
3. **Subscription persistence** — subscriptions are re-registered after every reconnection.

This document is split into two phases:

- **Phase 1** — Add a test suite that pins the observable behavior of `MqttManager` so that any
  breakage introduced during the upgrade is caught immediately.
- **Phase 2** — Design a drop-in `IManagedMqttClient`-compatible replacement built on top of the
  v5 `IMqttClient`, and outline the implementation steps.

---

## Phase 1: Test Suite

### 1.1 Goals

- Cover every public behavior of `MqttManager` that callers depend on.
- Keep tests fast and hermetic (no real broker required).
- Avoid changes to production code wherever possible; where a seam is needed, prefer the smallest
  possible change.

### 1.2 What Needs to Be Tested

The following behaviors are exercised by callers across the plugin:

| Behavior | Called from |
|---|---|
| `IsConnected` / `IsStarted` — delegate to underlying client state | `MainWindow`, `Ipc` |
| `ConnectToBroker()` — builds options, starts the client, enqueues connected message | `Ffxiv2Mqtt.cs` |
| `DisconnectFromBroker()` — sends will message disconnect, then stops | `MainWindow`, `Ffxiv2Mqtt.cs` |
| `PublishMessage(topic, payload, retain)` — builds topic, selects QoS, enqueues | `Topic.cs`, `Ffxiv2Mqtt.cs`, `Ipc` |
| `PublishMessage(topic, payload, retain, qos)` — explicit QoS override | `Ipc` |
| `ConfigureSubscribedTopics()` — subscribes/unsubscribes based on `OutputChannels` | `MqttManager` ctor, `MainWindow` |
| `AddMessageReceivedHandler()` — registers handler on the client event | `MqttManager` ctor, `Ipc` |
| `ComparePattern()` — MQTT wildcard matching (`#`, `+`, exact) | `ConfiguredMessageReceivedHandler` |
| Inbound routing — received messages are dispatched to the correct `OutputChannel` | internal |
| `Dispose()` — calls `DisconnectFromBroker` if connected, unregisters events, disposes client | `Ffxiv2Mqtt.cs` |

### 1.3 Required Seam: Logger Injection

`MqttManager` currently calls `Service.Log` (a static reference to the Dalamud logger) which
cannot be resolved in a test context. The smallest change that fixes this is to accept an
`ILogger` parameter in the constructor and store it instead of calling `Service.Log` directly.

Concretely:

```csharp
// Before (MqttManager.cs)
Service.Log.Information("Initializing MQTTManager");

// After
private readonly ILogger log;

public MqttManager(Configuration configuration, ILogger log)
{
    this.log = log;
    ...
    log.Information("Initializing MQTTManager");
}
```

The production call site in `Ffxiv2Mqtt.cs`:

```csharp
Service.MqttManager = new MqttManager(Configuration, Service.Log);
```

No other production code changes are needed for Phase 1.

### 1.4 Required Seam: IManagedMqttClient Injection

`MqttManager` currently creates its own `IManagedMqttClient` via `MqttFactory` in the
constructor. To mock it in tests the client must be injectable. Two equivalent options:

**Option A — constructor injection (recommended)**

```csharp
public MqttManager(Configuration configuration, ILogger log, IManagedMqttClient client)
{ ... }

// Production call site
var client = new MqttFactory().CreateManagedMqttClient();
Service.MqttManager = new MqttManager(Configuration, Service.Log, client);
```

**Option B — factory delegate injection**

```csharp
public MqttManager(Configuration configuration, ILogger log,
                   Func<IManagedMqttClient>? clientFactory = null)
{
    mqttClient = (clientFactory ?? (() => new MqttFactory().CreateManagedMqttClient()))();
}
```

Option A is cleaner and requires no `MqttFactory` in production path once the v5 replacement is
in place.

### 1.5 Test Project Setup

Create a new C# project alongside the plugin project:

```
ffxiv2Mqtt.Tests/
    ffxiv2Mqtt.Tests.csproj
    MqttManagerTests.cs
    ComparePatternTests.cs
    InboundRoutingTests.cs
```

**`ffxiv2Mqtt.Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../ffxiv2Mqtt/Ffxiv2Mqtt.csproj" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />
    <!-- MQTTnet same version as production to share the interface types -->
    <PackageReference Include="MQTTnet" Version="4.3.6.1152" />
    <PackageReference Include="MQTTnet.Extensions.ManagedClient" Version="4.3.6.1152" />
  </ItemGroup>
</Project>
```

### 1.6 Detailed Test Cases

#### `ComparePatternTests.cs`

`ComparePattern` is already a pure static method with no dependencies. No seams are needed.

```
Exact match — identical paths return true
Exact mismatch — different paths return false
Single-level wildcard (+) — matches one segment, not two
Multi-level wildcard (#) — matches any suffix including empty
Wildcard at end after prefix — prefix must still match
Pattern shorter than topic — returns true (pattern runs out first)
Pattern longer than topic — returns false (i > compare.Length guard)
```

Edge-cases to verify the existing off-by-one bug (index `i > compare.Length` instead of
`i >= compare.Length`):

```
Topic length exactly equals pattern length — should return true
Topic shorter than pattern — should return false
```

Note: the current guard on line 129 reads `if (i > compare.Length)` which uses `>` rather than
`>=`. When `i == compare.Length` the next access `compare[i]` will throw
`IndexOutOfRangeException`. A test exercising a topic that is one segment shorter than the
pattern will expose this bug. Document the expected correct behavior and fix the guard as part of
adding the test.

#### `MqttManagerTests.cs` — using a substituted `IManagedMqttClient`

Each test creates a fresh `NSubstitute` substitute for `IManagedMqttClient` and for `ILogger`,
then constructs `MqttManager`.

**State delegation**

```
IsConnected returns false when client.IsConnected is false
IsConnected returns true when client.IsConnected is true
IsStarted returns false when client.IsStarted is false
IsStarted returns true when client.IsStarted is true
```

**ConnectToBroker**

```
Does nothing (no StartAsync call) when BrokerAddress is empty string
Calls StartAsync exactly once with broker address, port, clientId, credentials from config
StartAsync options use MQTT protocol v5
StartAsync options include will topic built as "{baseTopic}/connected" with payload "false" and retain flag
EnqueueAsync is called once after StartAsync with topic "{baseTopic}/connected", payload "true", retain flag, QoS AtLeastOnce
When IncludeClientId is true, topic is built as "{baseTopic}/{clientId}/connected"
```

**PublishMessage**

```
Returns false without calling EnqueueAsync when IsConnected is false
Returns true and calls EnqueueAsync when IsConnected is true
Topic is built as "{baseTopic}/{inputTopic}"
Topic includes clientId segment when IncludeClientId is true
retain=false → QoS is ExactlyOnce, retain flag not set
retain=true  → QoS is AtLeastOnce, retain flag is set
Explicit QoS overload passes QoS through unchanged
EnqueueAsync ArgumentNullException is caught and returns false
```

**DisconnectFromBroker**

```
Calls InternalClient.DisconnectAsync with DisconnectWithWillMessage reason
Calls StopAsync(false) after disconnect
```

**ConfigureSubscribedTopics**

```
Calls SubscribeAsync for each enabled OutputChannel not already in CurrentSubscriptions
Does not call SubscribeAsync for a channel already in CurrentSubscriptions
Calls UnsubscribeAsync for topics in CurrentSubscriptions that are no longer in OutputChannels
Channels with ChannelType == Disabled are not subscribed
After call, CurrentSubscriptions reflects the new set
```

**AddMessageReceivedHandler**

```
Delegates subscribe to ApplicationMessageReceivedAsync event on the client
Exception during registration is caught and logged, does not propagate
```

**Dispose**

```
Calls DisconnectFromBroker when IsConnected is true
Does not call DisconnectFromBroker when IsConnected is false
Unregisters ConnectedAsync, ConnectingFailedAsync, DisconnectedAsync handlers
Calls mqttClient.Dispose()
```

#### `InboundRoutingTests.cs`

These tests drive the private `ConfiguredMessageReceivedHandler` indirectly via
`AddMessageReceivedHandler`. A test helper captures the registered handler during construction
and invokes it with a synthesized `MqttApplicationMessageReceivedEventArgs`.

Because `MqttApplicationMessageReceivedEventArgs` does not have a public constructor in v4 it
must be constructed via reflection or via a helper that builds a
`MqttApplicationMessage` and uses the package's internal factory. Alternatively, extract
`ConfiguredMessageReceivedHandler` into its own `internal` static method that accepts plain
parameters (topic string, payload string, channel list) so it can be tested directly.

**Scenarios**

```
Exact topic match routes payload to the matched channel
Topic that matches a + wildcard channel routes correctly
Topic that matches a # wildcard channel routes correctly
Topic that matches no channel is ignored (no output calls made)
Disabled channel is ignored even if topic matches
Multiple channels can match the same message
```

The output operations (`ChatGui.Print`, `ToastGui.ShowNormal`, `NotificationManager.AddNotification`)
must be substituted. These come from `Service.*` statics. If the logger injection seam approach
is extended to these services, they can be substituted. Alternatively, refactor the routing
logic into a separate `MessageRouter` class with injected output sinks so it can be tested
without Dalamud at all.

---

## Phase 2: Custom Managed Client for MQTTnet v5

### 2.1 What the Managed Client Did

The `IManagedMqttClient` from `MQTTnet.Extensions.ManagedClient` provided:

| Capability | Mechanism |
|---|---|
| Auto-reconnect | Internal retry loop with backoff, reconnects after `DisconnectedAsync` |
| Message queuing | Internal `ConcurrentQueue`; `EnqueueAsync` accepted messages while offline |
| Draining the queue | Background task that dequeues and calls `PublishAsync` when connected |
| Subscription persistence | Maintained a subscription list; re-subscribed after every reconnect |
| Lifecycle: `StartAsync` / `StopAsync` | Started/stopped the internal loop and the underlying `IMqttClient` |
| `IsStarted` property | Separate from `IsConnected`; true once `StartAsync` succeeds |
| Events forwarded | `ConnectedAsync`, `ConnectingFailedAsync`, `DisconnectedAsync`, `ApplicationMessageReceivedAsync` |
| `InternalClient` property | Exposed the underlying `IMqttClient` (used for `DisconnectAsync` with will) |

### 2.2 v5 Differences Relevant to the Replacement

- `IMqttClient` in v5 retains the same core `ConnectAsync`, `DisconnectAsync`, `PublishAsync`,
  `SubscribeAsync`, `UnsubscribeAsync`, and event pattern.
- `MqttFactory.CreateMqttClient()` still exists.
- `TryPingAsync()` is a new convenience method for health checks.
- `MqttClientDisconnectOptionsReason.DisconnectWithWillMessage` is still present.
- Namespace changes: most types move from `MQTTnet.Client.*` but the public surface is otherwise
  very similar.
- `EnqueueAsync` no longer exists — callers must call `PublishAsync` directly.
- `StartAsync` / `StopAsync` no longer exist — callers manage the lifecycle directly.
- `IsStarted` no longer exists as a built-in property.

### 2.3 Proposed Interface

Define the replacement interface to match exactly the members used by `MqttManager`:

```csharp
namespace Ffxiv2Mqtt.Mqtt;

/// <summary>
/// Drop-in replacement for IManagedMqttClient supporting auto-reconnect,
/// message queuing, and subscription persistence.
/// </summary>
public interface IManagedMqttClientV5 : IDisposable
{
    bool IsConnected { get; }
    bool IsStarted   { get; }

    IMqttClient InternalClient { get; }

    event Func<MqttClientConnectedEventArgs,   Task> ConnectedAsync;
    event Func<ConnectingFailedEventArgs,       Task> ConnectingFailedAsync;
    event Func<MqttClientDisconnectedEventArgs, Task> DisconnectedAsync;
    event Func<MqttApplicationMessageReceivedEventArgs, Task> ApplicationMessageReceivedAsync;

    Task StartAsync(ManagedMqttClientOptions options);
    Task StopAsync(bool cleanDisconnect = true);

    Task EnqueueAsync(MqttApplicationMessage message);

    Task SubscribeAsync(string topic,
                        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce);
    Task UnsubscribeAsync(string topic);
}
```

`ManagedMqttClientOptions` is a thin DTO:

```csharp
public sealed class ManagedMqttClientOptions
{
    public MqttClientOptions ClientOptions { get; init; }  // required
    public int MaxPendingMessages { get; init; } = 1000;
    public TimeSpan AutoReconnectDelay { get; init; } = TimeSpan.FromSeconds(5);
}

public sealed class ManagedMqttClientOptionsBuilder
{
    private MqttClientOptions? _clientOptions;

    public ManagedMqttClientOptionsBuilder WithClientOptions(MqttClientOptions o)
    { _clientOptions = o; return this; }

    public ManagedMqttClientOptions Build()
        => new() { ClientOptions = _clientOptions! };
}
```

### 2.4 Implementation: `ManagedMqttClientV5`

```
ffxiv2Mqtt/
    Mqtt/
        IManagedMqttClientV5.cs
        ManagedMqttClientOptions.cs
        ManagedMqttClientOptionsBuilder.cs
        ManagedMqttClientV5.cs
```

**Internal state**

```csharp
private IMqttClient _client;              // underlying v5 client
private ManagedMqttClientOptions? _options;
private Channel<MqttApplicationMessage> _queue;   // bounded by MaxPendingMessages
private HashSet<(string topic, MqttQualityOfServiceLevel qos)> _subscriptions;
private CancellationTokenSource _cts;
private Task? _pumpTask;
private bool _started;
```

**`StartAsync`**

1. Store `options`.
2. Create a bounded `Channel<MqttApplicationMessage>` with capacity `options.MaxPendingMessages`
   and `BoundedChannelFullMode.DropOldest` (mirrors v4 behavior).
3. Set `_started = true`.
4. Start the reconnect/pump loop as a background `Task` stored in `_pumpTask`.

**Reconnect/pump loop** (runs until `StopAsync`)

```
while (!cancellationToken.IsCancellationRequested):
    if not connected:
        try:
            await _client.ConnectAsync(_options.ClientOptions, ct)
            await ResubscribeAllAsync()
            raise ConnectedAsync event
        except:
            raise ConnectingFailedAsync event
            await Task.Delay(_options.AutoReconnectDelay, ct)
            continue
    // drain the queue
    while _queue.TryRead(out var message):
        try:
            await _client.PublishAsync(message, ct)
        except:
            // re-enqueue or discard based on QoS
    await Task.Delay(small polling interval, ct)  // or use a SemaphoreSlim/event
```

A better pattern than polling avoids unnecessary CPU use:

- Use a `SemaphoreSlim(0)` as a "work available" signal.
- `EnqueueAsync` releases the semaphore after writing to the channel.
- The pump `WaitAsync`es on the semaphore.
- The `DisconnectedAsync` event from the underlying client triggers the reconnect path.

**`EnqueueAsync`**

```csharp
public async Task EnqueueAsync(MqttApplicationMessage message)
{
    if (!_started) throw new InvalidOperationException("Call StartAsync first.");
    await _queue.Writer.WriteAsync(message);   // blocks if full (or use TryWrite to drop)
    _workAvailable.Release();
}
```

**`StopAsync`**

1. If `cleanDisconnect`, flush any remaining queued messages before disconnecting.
2. Cancel `_cts`.
3. Await `_pumpTask`.
4. If still connected, call `_client.DisconnectAsync()`.
5. Set `_started = false`.

**`SubscribeAsync` / `UnsubscribeAsync`**

- Add/remove from `_subscriptions` set.
- If currently connected, forward to `_client.SubscribeAsync` / `_client.UnsubscribeAsync`.
- If disconnected, the `ResubscribeAllAsync` call on next connect will pick up the full set.

**`ResubscribeAllAsync`**

```csharp
private async Task ResubscribeAllAsync()
{
    foreach (var (topic, qos) in _subscriptions)
        await _client.SubscribeAsync(topic, qos);
}
```

**`DisconnectedAsync` forwarding**

Subscribe to `_client.DisconnectedAsync` internally. Forward the event to external listeners
and signal the reconnect loop.

**`InternalClient`**

```csharp
public IMqttClient InternalClient => _client;
```

This preserves the `DisconnectFromBroker` path which calls:
```csharp
mqttClient.InternalClient.DisconnectAsync(MqttClientDisconnectOptionsReason.DisconnectWithWillMessage)
```

### 2.5 Integration with `MqttManager`

Once `ManagedMqttClientV5` is complete, `MqttManager` changes are limited to:

1. Replace `using MQTTnet.Extensions.ManagedClient;` with `using Ffxiv2Mqtt.Mqtt;`.
2. Change the field type from `IManagedMqttClient` to `IManagedMqttClientV5`.
3. Update the production factory call from `mqttFactory.CreateManagedMqttClient()` to
   `new ManagedMqttClientV5(mqttFactory.CreateMqttClient())`.
4. Remove the `MQTTnet.Extensions.ManagedClient` package reference from the `.csproj`.

No callers of `MqttManager`'s public API change at all.

### 2.6 Implementation Steps (Ordered)

The following steps should be taken in order. The test suite written in Phase 1 acts as a
regression harness throughout.

1. **Upgrade `MQTTnet` package** from `4.3.6.1152` to `5.*` and remove
   `MQTTnet.Extensions.ManagedClient`. Fix any namespace-level compilation errors (most are
   mechanical renames).

2. **Add the `Mqtt/` folder** with `IManagedMqttClientV5.cs` and the options types. The interface
   must compile against v5 types.

3. **Implement `ManagedMqttClientV5`** starting with a minimal version that connects, publishes,
   and subscribes (no queue, no reconnect). Get the tests green with this skeleton.

4. **Add the message queue** (`Channel<MqttApplicationMessage>` + `EnqueueAsync`). The queue
   should behave identically to v4: messages are accepted while disconnected, drained in order
   when connected.

5. **Add the reconnect loop** using the timer/ping pattern from the v5 connection samples. Test
   by simulating a disconnection event via the mocked `IMqttClient.DisconnectedAsync`.

6. **Add subscription persistence** in `ResubscribeAllAsync`. Verify by subscribing, simulating a
   reconnect, and asserting that `_client.SubscribeAsync` was called again for each topic.

7. **Wire up `InternalClient`** and verify the existing `DisconnectFromBroker` path still works.

8. **Update `MqttManager`** to use `IManagedMqttClientV5`. Run the Phase 1 test suite — all
   tests should remain green because the interface contract is identical.

9. **Remove the `MQTTnet.Extensions.ManagedClient` package** from the `.csproj`.

10. **Manual/integration test** against a live broker to confirm end-to-end behavior.

### 2.7 Files to Create

| File | Purpose |
|---|---|
| `ffxiv2Mqtt/Mqtt/IManagedMqttClientV5.cs` | Interface definition |
| `ffxiv2Mqtt/Mqtt/ManagedMqttClientOptions.cs` | Options DTO |
| `ffxiv2Mqtt/Mqtt/ManagedMqttClientOptionsBuilder.cs` | Fluent builder |
| `ffxiv2Mqtt/Mqtt/ManagedMqttClientV5.cs` | Implementation |
| `ffxiv2Mqtt.Tests/ffxiv2Mqtt.Tests.csproj` | Test project |
| `ffxiv2Mqtt.Tests/ComparePatternTests.cs` | Pure unit tests for wildcard matching |
| `ffxiv2Mqtt.Tests/MqttManagerTests.cs` | Tests for MqttManager using mocked client |
| `ffxiv2Mqtt.Tests/InboundRoutingTests.cs` | Tests for inbound message routing |
| `ffxiv2Mqtt.Tests/ManagedMqttClientV5Tests.cs` | Tests for the custom managed client |

### 2.8 Known Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Dalamud.NET.Sdk may pin a specific MQTTnet version or have its own bundled copy | Check Dalamud's shipped assemblies before upgrading; may need to embed MQTTnet privately |
| `Service.*` static coupling makes unit testing hard | Logger injection seam (Phase 1 step 1.3) is the first priority; consider further refactoring for output channel sinks |
| Pump task exception handling | Any unhandled exception in the background task will silently swallow errors; use `TaskScheduler.UnobservedTaskException` or explicit try/catch with logging inside the loop |
| Message ordering under reconnect | The `Channel<T>` preserves FIFO order, but messages published during the reconnect window are deferred, not lost |
| Thread safety of `_subscriptions` | Use `ConcurrentDictionary<string, MqttQualityOfServiceLevel>` instead of `HashSet` if subscriptions can be modified concurrently |
