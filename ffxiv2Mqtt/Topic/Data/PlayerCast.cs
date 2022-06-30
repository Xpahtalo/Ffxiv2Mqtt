using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class PlayerCast : Topic, IUpdatable
    {
        public bool IsCasting { get => isCasting; }
        public uint CastId { get => DalamudServices.ClientState.LocalPlayer.CastActionId; }
        public float CastTime { get => DalamudServices.ClientState.LocalPlayer.TotalCastTime; }
        public float RemainingTime { get => CastTime - DalamudServices.ClientState.LocalPlayer.CurrentCastTime; }


        bool isCasting;

        public PlayerCast(MqttManager m) : base(m)
        {
            topic = "Player/Casting";
        }

        public void Update()
        {
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            TestValue(localPlayer.IsCasting, ref isCasting);

            PublishIfNeeded();
        }
    }
}
