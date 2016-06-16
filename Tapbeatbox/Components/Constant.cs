using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapbeatbox
{
    class Constant
    { 
        public const int defaultVolume = 5; //Default volume for slots and master   
        public const int maxVolume = 10; //Maximum possible volumeI:\Raw\CS\Sem5\Software Project\tapbeatbox_27_05_2016\Tapbeatbox\Tapbeatbox\Components\Constant.cs
        public const int minVolume = 0; //Minimum possible volume

        public const double valueGap = 0.005; //Value gap which detect as a tap - this is a sensitivity value
        public static TimeSpan timeGap = new TimeSpan(0, 0, 0, 0, 100); //mimimum required time gap to detect as two taps

        public const int parmCount = 6;
        public const int trainingTapCount = 10;

        public const int neuralNetFeedCount = 1000;
        public const string networkURI = "http://tapbeatbox-globalgoals.rhcloud.com/rest/data/add";

        public const int mediaCount = 4;
    }
}
