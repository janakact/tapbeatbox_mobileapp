using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapbeatbox.TapLibrary
{
    public class TapRecognizer
    {
        private List<Slot> listOfSlots;

        public TapRecognizer(List<Slot> slots)
        {
            listOfSlots = slots;
        }


        //Get the maximum value
        public int recognizeTheSlot(double[] parms)
        {
            int ret = 0;
            for(int i=1; i<Constant.parmCount; i++)
            {
                if (parms[i] > parms[ret])
                    ret = i;
            }
            return ret;
        }



    }
}
