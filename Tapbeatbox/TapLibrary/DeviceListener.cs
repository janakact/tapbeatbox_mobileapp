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
        private DataSet currentDataSet;
        
        double lastValue = 0; //Last reading
        DateTime lastTime ; // LastTime of the reading
        DateTime startTime;


        //For running 
        bool running = false;
        private DeviceListener()
        {
            sensor = new AccelerometerSensor();
            lastTime =  DateTime.Now;
        }

        //Singleton object generation function
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

        //This function is to start listening
        public void run()
        {
            ThreadPool.RunAsync(instance.runThread);
        }
        public void stop()
        {
            running = false;
        }

        //This is the function for thread to run
        public void runThread(Object threadContext)
        {
            currentDataSet = getInitialDataSet(); // DataSet -----------------
            startTime = DateTime.Now; //-------------------------------------
            running = true;
            while (running)
            {
                double[] readings = sensor.getReadings();

                TimeSpan t = (DateTime.Now - startTime);
                currentDataSet.dataList.Add(new Data() { x = readings[0], y = readings[1], z = readings[2], time=(int)t.TotalMilliseconds});//add to dataset

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

        private static DataSet getInitialDataSet()
        {
            DataSet d = new DataSet() {deviceId=5,deviceModel="Lumia 640"};
            return d;
        
        }

        public DataSet getCurrentDataSet()
        {
            return currentDataSet;
        }
    }
}
