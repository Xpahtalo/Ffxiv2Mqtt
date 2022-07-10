using Dalamud.Game.ClientState.Statuses;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data
{
    internal class PlayerStatuses : Topic, IDisposable
    {
        [PluginService]
        // ReSharper disable once MemberCanBePrivate.Global
        public PlayerEvents? PlayerEvents { get; set; }

        private readonly JsonSerializerOptions serializerOptions;
        private          int                   statusCount;

        protected override string TopicPath => "Player/Status";
        protected override bool   Retained  => false;

        // Setting up a the JsonSerializerOptions takes some time, so saving it here for reuse makes things much faster.
        public PlayerStatuses()
        {
            serializerOptions = new JsonSerializerOptions
                                {
                                    Converters = { new StatusSerializer() },
                                };
        }
        
        public override void Initialize()
        {
            if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
        }

        // Publish a message if the number of statuses on the player has changed.
        private void PlayerUpdated(PlayerCharacter localPlayer)
        {
            var shouldPublish = false;

            var activeStatusCount = localPlayer.StatusList.Count<Status>(status => status.Address != IntPtr.Zero);
            TestValue(activeStatusCount, ref statusCount, ref shouldPublish);
            if (shouldPublish) {
                Publish(JsonSerializer.Serialize(localPlayer.StatusList as IReadOnlyCollection<Status>, serializerOptions));
            }
        }

        public void Dispose()
        {
            if (PlayerEvents is not null) PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
        }

        // A custom serializer is required to safely read a status because it has many properties that don't make sense to send.
        private class StatusSerializer : JsonConverter<Status>
        {
            public override Status Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Status value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("Name", value.GameData.Name.ToString());
                writer.WriteNumber("StatusId",      value.StatusId);
                writer.WriteNumber("Param",         value.Param);
                writer.WriteNumber("RemainingTime", value.RemainingTime);
                writer.WriteNumber("SourceID",      value.SourceID);
                writer.WriteNumber("StackCount",    value.StackCount);
                writer.WriteEndObject();
            }
        }
    }
}
