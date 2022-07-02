using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class PlayerGathererStatsTopic : Topic, IUpdatable
    {
        public uint GP { get => gp; }
        uint gp;
        Job previousJob;

        internal PlayerGathererStatsTopic(MqttManager mqttManager) : base(mqttManager)
        {
            topic = "Player/Gatherer/CurrentStats";
        }

        public void Update()
        {
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;

            if (previousJob.IsGatherer() == false
                && ((Job)localPlayer.ClassJob.Id).IsGatherer())
            {
                needsPublishing = true;
            }
            else
            {
                TestValue((Job)localPlayer.ClassJob.Id, ref previousJob);
            }
            previousJob = (Job)localPlayer.ClassJob.Id;


            if (localPlayer.MaxGp != 0)
                TestValue(localPlayer.CurrentGp, ref gp);

            PublishIfNeeded();
        }
    }
}
