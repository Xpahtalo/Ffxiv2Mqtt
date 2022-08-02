using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt;

public record OutputChannel
{
    public string            Path       { get; init; } = default!;
    public OutputChannelType ChannelType { get; init; } = default!;
}
