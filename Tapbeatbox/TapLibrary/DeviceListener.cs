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
        private DeviceListener()
        { }

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
                TapEventArgs e = new TapEventArgs(1, 1);
                OnTap(this, e);
            }
        }
    }
}
