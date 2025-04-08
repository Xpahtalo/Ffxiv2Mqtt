﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Statuses;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class PlayerStatuses : Topic, IDisposable
{
    private          int                   statusCount;
    private          int                   stackCount;
    private readonly JsonSerializerOptions serializerOptions;

    protected override string TopicPath => "Player/Status";
    protected override bool   Retained  => false;


    // Setting up the JsonSerializerOptions takes some time, so saving it here for reuse makes things much faster.
    public PlayerStatuses()
    {
        serializerOptions = new JsonSerializerOptions
                            {
                                Converters = { new StatusSerializer() },
                            };
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    // Publish a message if the number of statuses on the player has changed.
    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        var shouldPublish     = false;
        var activeStatusCount = 0;
        var stacks            = 0;

        foreach (var status in localPlayer.StatusList) {
            if (status.Address == IntPtr.Zero) continue;
            activeStatusCount++;
            stacks += status.Param;
        }

        TestValue(activeStatusCount, ref statusCount, ref shouldPublish);
        TestValue(stacks,            ref stackCount,  ref shouldPublish);
        if (shouldPublish)
            Publish(JsonSerializer.Serialize(localPlayer.StatusList, serializerOptions));
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }

    // A custom serializer is required to safely read a status because it has many properties that don't make sense to send.
    private class StatusSerializer : JsonConverter<Status>
    {
        public override Status Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) { throw new NotImplementedException(); }

        public override void Write(Utf8JsonWriter writer, Status value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", value.GameData.Value.Name);
            writer.WriteNumber("StatusId",      value.StatusId);
            writer.WriteNumber("Param",         value.Param);
            writer.WriteNumber("RemainingTime", value.RemainingTime);
            writer.WriteNumber("SourceID",      value.SourceId);
            writer.WriteNumber("Parameter",    value.Param);
            writer.WriteEndObject();
        }
    }
}
