using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tapbeatbox.TapLibrary
{
    [DataContract]
    public class DataSet
    {
        [DataMember(Name = "dataList")]
        public List<Data> dataList { get; set; }

        [DataMember(Name ="setId")]
        public int setId; //Unique for the set

        [DataMember(Name = "slotId")]
        public int slotId; //Specify the slot


        [DataMember(Name = "deviceId")]
        public int deviceId;

        [DataMember(Name = "deviceModel")]
        public string deviceModel;
        //public string id;

        public DataSet()
        {
            dataList = new List<Data>();
        }
    }

    [DataContract]
    public class Data
    {
        [DataMember(Name = "time")]
        public int time { get; set; }

        [DataMember(Name = "x")]
        public double x { get; set; }

        [DataMember(Name = "y")]
        public double y { get; set; }

        [DataMember(Name = "z")]
        public double z { get; set; }
    }
}
