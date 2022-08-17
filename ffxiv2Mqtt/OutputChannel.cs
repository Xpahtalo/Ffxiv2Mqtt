using Ffxiv2Mqtt.Enums;

namespace Ffxiv2Mqtt;

public record OutputChannel
{
    public string            Path        { get; set; } = default!;
    public OutputChannelType ChannelType { get; set; } = default!;
}
