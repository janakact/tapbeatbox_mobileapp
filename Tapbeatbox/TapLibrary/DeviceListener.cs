using System;
using System.IO;
using System.Collections.Generic;

using System.Threading;
using Windows.System.Threading;
using System.Diagnostics;

namespace Tapbeatbox.TapLibrary
{
    class DeviceListener
    {
        //private List<Slot> slots;
        Sensor sensor;
        
        double lastValue = 0;
        DateTime lastTime ;
        private DeviceListener()
        {
            sensor = new AccelerometerSensor();
            lastTime =  DateTime.Now;
        }

        public static DeviceListener getInstance()
        {
            if (instance == null) instance = new DeviceListener();
            return instance;
        }

        public event EventHandler<TapEventArgs> OnTap;

        //Instance
        private static DeviceListener instance;
        public class TapEventArgs : EventArgs
        {
            private string message;
            int slotId;
            int present;
            public TapEventArgs(int slotId, int present)
            {
                this.slotId = slotId;
                this.present = present;
            }
        }

        public void run()
        {
            ThreadPool.RunAsync(instance.runThread);
        }

        public void runThread(Object threadContext)
        {
            while (true)
            {
                double[] readings = sensor.getReadings();

                double sum = readings[0] * readings[0] + readings[1] * readings[1] + readings[2] * readings[2];
                if(Math.Abs(lastValue - sum) > Constant.valueGap && (DateTime.Now-lastTime)>Constant.timeGap)
                {
                    TapEventArgs e = new TapEventArgs(1, 1);
                    OnTap(this, e);
                    lastTime = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine(lastValue-sum);
                }
                System.Diagnostics.Debug.WriteLine(lastValue - sum);
                lastValue = sum;
            }
        }
    }
}
