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
        double[] lastValues; //Keep all values

        DateTime lastTime ; // LastTime of the reading
        DateTime startTime;


        //For running 
        bool running = false;

        private DeviceListener()
        {
            sensor = new AccelerometerSensor();
            lastTime =  DateTime.Now;

            //To keep last time value;
            lastValue = 0;
            lastValues = new double[Constant.parmCount];
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
            public int present;
            public double[] parms;
            public TapEventArgs(int present,double[] parms)
            {
                this.present = present;
                this.parms = parms;
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
        private void runThread(Object threadContext)
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
               // System.Diagnostics.Debug.WriteLine(t + " : " + (Math.Abs(lastValue - sum) / Constant.timeGap.Milliseconds));
                if ((Math.Abs(lastValue - sum)/Constant.timeGap.Milliseconds) > Constant.valueGap && (DateTime.Now-lastTime)>Constant.timeGap)
                {

                    TapEventArgs e = new TapEventArgs(1, convertToParms(lastValues,readings)); 
                    OnTap(this, e);
                    lastTime = DateTime.Now;
                }

                //Update last time readings
                lastValue = sum;
                for(int i=0; i<3; i++)
                    lastValues[i] = readings[i];
                

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
        private double[] convertToParms(double[] lastReadings, double[] readings)
        {
            double[] parms = new double[Constant.parmCount];
            for (int i = 0; i < 3; i++)
            {
                parms[i] = (readings[i] - lastValues[i]) * 10;
                parms[i + 3] = (readings[i] - lastValues[i])*(readings[(i+1)%3] - lastValues[(i + 1) % 3]) * 10;
            }
            
            return parms;
        }
    }

    
}
