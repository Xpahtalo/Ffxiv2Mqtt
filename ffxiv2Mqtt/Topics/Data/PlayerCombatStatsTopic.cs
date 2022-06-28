﻿using Ffxiv2Mqtt.TopicTracker.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal unsafe class PlayerCombatStatsTopic : Topic, IUpdatable
    {
        public uint HP { get => hp; }
        public uint MP { get => mp; }
        public byte Shield { get => shield; }

        uint hp;
        uint mp;
        byte shield;

        internal PlayerCombatStatsTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Combat/CurrentStats";
        }

        public void Update()
        {
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;

            TestValue(localPlayer.CurrentHp, ref hp);
            TestValue(localPlayer.CurrentMp, ref mp);
            TestValue(((Character*)localPlayer.Address)->ShieldValue, ref shield);

            PublishIfNeeded();
        }
    }
}
