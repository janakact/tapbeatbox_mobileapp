using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace Tapbeatbox.TapLibrary
{
    class AccelerometerSensor : Sensor
    {

        Accelerometer _accelerometer;

        public AccelerometerSensor()
        {
            _accelerometer = Accelerometer.GetDefault();
        }

        public int getNumberOfAxis()
        {
            return 3;
        }

        public double[] getReadings()
        {
            double[] a = new double[3];
            if (_accelerometer == null) return a;
            AccelerometerReading reading = _accelerometer.GetCurrentReading();
            a[0] = reading.AccelerationX;
            a[1] = reading.AccelerationY;
            a[2] = reading.AccelerationZ;
            return a;
        }
    }
}
