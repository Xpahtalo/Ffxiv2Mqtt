using System;
using System.Text.Json;
using Dalamud.Game.ClientState.Conditions;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal sealed class PlayerConditions : Topic, IDisposable
{
    protected override string TopicPath => "Player/Conditions";
    protected override bool   Retained  => false;

    public PlayerConditions() { Service.Conditions.ConditionChange += ConditionChange; }

    // Publish to each condition's topic whenever the state changes.
    private void ConditionChange(ConditionFlag flag, bool value)
    {
        Publish($"{TopicPath}/{flag}", JsonSerializer.Serialize(new
                                                                {
                                                                    Id     = (int)flag,
                                                                    Active = value,
                                                                }));
    }

    public void Dispose() { Service.Conditions.ConditionChange -= ConditionChange; }
}
