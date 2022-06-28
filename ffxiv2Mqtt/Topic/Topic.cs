using Dalamud.Logging;
using Newtonsoft.Json;
using System;

namespace Ffxiv2Mqtt.Topic
{
    internal class Topic
    {
        private protected MqttManager mqttManager;
        private protected string topic;
        private protected bool needsPublishing;

        internal Topic(MqttManager mqttManager)
        {
            PluginLog.Verbose($"Creating {this.GetType().Name}");
            this.mqttManager = mqttManager;
            topic = "";
        }

        // In .NET 6 and C# 11, these can be simplified down to a single method with generics using INumber.
        private protected void TestCountUp(short current, ref short previous, int interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentLower = current < previous;
            bool exceededInterval = (current - previous) >= interval;

            if(needsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }
            
            if (reachedZero || noLongerZero || wentLower)
            {
                previous = current;
                needsPublishing = true;
                return;
            }
            else
            {
                if (interval >= 0)
                {
                    if (exceededInterval)
                    {
                        previous = current;
                        needsPublishing = true;
                    }
                }
                else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                { 
                    previous = current;
                }
            }
        }

        private protected void TestCountDown(ushort current, ref ushort previous, int interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentHigher = current > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (needsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentHigher)
            {
                previous = current;
                needsPublishing = true;
                return;
            }
            else
            {
                if (interval >= 0)
                {
                    if (exceededInterval)
                    {
                        previous = current;
                        needsPublishing = true;
                    }
                }
                else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }
        private protected void TestCountDown(short current, ref short previous, int interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentHigher = current > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (needsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentHigher)
            {
                previous = current;
                needsPublishing = true;
                return;
            }
            else
            {
                if (interval >= 0)
                {
                    if (exceededInterval)
                    {
                        previous = current;
                        needsPublishing = true;
                    }
                }
                else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }
        private protected void TestCountDown(int current, ref int previous, int interval)
        {
            bool reachedZero = (previous == 0) && (current != 0);
            bool noLongerZero = (previous != 0) && (current == 0);
            bool wentHigher = current > previous;
            bool exceededInterval = (previous - current) >= interval;

            if (needsPublishing) // If something else has caused an update, we should also update the timer value
            {
                previous = current;
                return;
            }

            if (reachedZero || noLongerZero || wentHigher)
            {
                previous = current;
                needsPublishing = true;
                return;
            }
            else
            {
                if (interval >= 0)
                {
                    if (exceededInterval)
                    {
                        previous = current;
                        needsPublishing = true;
                    }
                }
                else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                {
                    previous = current;
                }
            }
        }
        
        private protected void TestValue<T>(T current, ref T previous)
        {
            if (!current!.Equals(previous))
            {
                previous = current;
                needsPublishing = true;
            }
            else // This is done so that the timer value will be accurate whenever the topic gets updated for any other reason
                previous = current;            
        }

        
        internal virtual void PublishIfNeeded()
        {
            if (needsPublishing)
            {
                Publish();
                needsPublishing = false;
            }
        }
        internal virtual void Publish(bool retained = false)
        {
            mqttManager.PublishMessage(topic, JsonConvert.SerializeObject(this), retained);
        }
        internal virtual void Publish(Object o, bool retained = false)
        {
            mqttManager.PublishMessage(topic, JsonConvert.SerializeObject(o), retained);
        }
    }
}
