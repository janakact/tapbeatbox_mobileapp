using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tapbeatbox.TapLibrary;
using Windows.UI.Core;

namespace Tapbeatbox
{
    public class ToneSlot: INotifyPropertyChanged,Slot
    {
        private int id;
        public int ID { get { return id; } set { id = value; } }

        private string name;
        public string Name { get { return name; }set { name = value; NotifyPropertyChanged(); } }

        private int volume;
        public int Volume { get { return volume; } set { volume = value; NotifyPropertyChanged();
                
            } }

        private string toneName;
        public string ToneName { get { return toneName; } set { toneName = value; NotifyPropertyChanged(); } }

        public List<double[]> trainingDataSet = new List<double[]>();


        //This is to update the interface when a value of the object is changed
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}
