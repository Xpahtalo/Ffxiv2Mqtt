using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal interface ICleanable
    {
        void Cleanup();
    }
}
