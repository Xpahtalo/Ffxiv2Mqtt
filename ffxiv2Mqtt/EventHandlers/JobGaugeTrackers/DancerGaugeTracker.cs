using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class DancerGaugeTracker : BaseGaugeTracker
    {
        public byte completedSteps;
        public byte esprit;
        public byte feathers;
        public bool isDancing;
        public uint nextStep;
        public uint[] steps;

        public DancerGaugeTracker(MqttManager m) : base(m)
        {
            steps = new uint[4];
        }

        public void Update(DNCGauge dancerGauge)
        {
            mqttManager.TestValue(dancerGauge.CompletedSteps, ref completedSteps, "JobGauge/DNC/CompletedSteps");
            mqttManager.TestValue(dancerGauge.Esprit, ref esprit, "JobGauge/DNC/Esprit");
            mqttManager.TestValue(dancerGauge.Feathers, ref feathers, "JobGauge/DNC/Feathers");
            mqttManager.TestValue(dancerGauge.IsDancing, ref isDancing, "JobGauge/DNC/IsDancing");
            mqttManager.TestValue(dancerGauge.NextStep, ref nextStep, "JobGauge/DNC/NextStep");
            for (int i = 0; i < steps.Length; i++)
            {
                mqttManager.TestValue(dancerGauge.Steps[i], ref steps[i], $"JobGauge/DNC/Step{i + 1}");
            }
        }
    }
}
