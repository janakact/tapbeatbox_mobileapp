using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Tapbeatbox.Components
{
    class ContententManager
    {
        Windows.Storage.ApplicationDataContainer localSettings;

        private ObservableCollection<ToneSlot> _listOfSlots = new ObservableCollection<ToneSlot>();

        public string DefaultToneName;

        public ObservableCollection<ToneSlot> ListOfSlots
        {
            get { return _listOfSlots; }
            set { _listOfSlots = value; }
        }


        private int _masterVolume;

        public int MasterVolume
        {
            get { return _masterVolume; }
            set { _masterVolume = value; }
        }

        private bool _isShareData;

        public bool IsShareData
        {
            get { return _isShareData; }
            set { _isShareData = value; }
        }


        public void addNewSlot()
        {
            int i = _listOfSlots.Count;
            _listOfSlots.Add(new ToneSlot { ID = i, Name = "Slot" + i, Volume = Constant.defaultVolume, ToneName = DefaultToneName });

        }

        public void removeSlot()
        {
            if(_listOfSlots.Count>1)
                _listOfSlots.RemoveAt(_listOfSlots.Count - 1);

        }

        public void loadData()
        {
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            //Load User Data
            dynamic a = localSettings.Values["SlotCount"];
            int slotCount = (a == null) ? 0 : (int)a;
            for (int i = 0; i < slotCount; i++)
            {
                ToneSlot s = LoadSlotItem(i);
                if (s != null) _listOfSlots.Add(s);
            }
            dynamic isShareData = localSettings.Values["IsShareData"];
            IsShareData = isShareData == 1 ? true : false;

            dynamic masterVolume = localSettings.Values["masterVolume"];
            MasterVolume = masterVolume == null ? Constant.defaultVolume : (int)masterVolume;

            //Default dummy slots
            if (slotCount == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    addNewSlot();
                    SaveSlotItem(i);
                }
            }


        }

        public void SaveGeneralData()
        {
            localSettings.Values["MasterVolume"] = this.MasterVolume;
            localSettings.Values["IsShareData"] = this.IsShareData ? 1 : 0;
        }

        //Load and Save Slot Items
        public void SaveSlotItem(int index)
        {
            if (index >= _listOfSlots.Count) return;
            Windows.Storage.ApplicationDataCompositeValue composite =
                new Windows.Storage.ApplicationDataCompositeValue();

            ToneSlot slot = _listOfSlots[index];
            composite["ID"] = slot.ID;
            composite["Name"] = slot.Name;
            composite["ToneName"] = slot.ToneName;
            composite["Volume"] = slot.Volume;

            localSettings.Values["Slot" + index] = composite;
            localSettings.Values["SlotCount"] = _listOfSlots.Count;

            SaveTrainingData(slot.ID, slot.trainingDataSet);

        }


        private ToneSlot LoadSlotItem(int index)
        {
            ToneSlot slot = new ToneSlot();
            Windows.Storage.ApplicationDataCompositeValue composite = localSettings.Values["Slot" + index] as Windows.Storage.ApplicationDataCompositeValue;

            if (composite == null) return null;

            slot.ID = (int)composite["ID"];
            slot.Name = (string)composite["Name"];
            slot.Volume = (int)composite["Volume"];
            try
            {
                slot.ToneName = (string)composite["ToneName"];
            }
            catch
            {
                slot.ToneName = ""; //Set empty string 
            }
            //slot.trainingDataSet = (List<double[]>)composite["DataSet"];
            LoadTrainingData(slot.ID, slot.trainingDataSet);
            return slot;
        }

        private async void SaveTrainingData(int index, List<double[]> dataList)
        {
            StorageFile userdetailsfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("DataList" + index, CreationCollisionOption.ReplaceExisting);

            IRandomAccessStream raStream = await userdetailsfile.OpenAsync(FileAccessMode.ReadWrite);

            using (IOutputStream outStream = raStream.GetOutputStreamAt(0))
            {

                // Serialize the Session State. 

                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Double[]>));

                serializer.WriteObject(outStream.AsStreamForWrite(), dataList);

                await outStream.FlushAsync();
                outStream.Dispose(); //  
                raStream.Dispose();
            }
        }

        private async void LoadTrainingData(int index, List<double[]> dataList)
        {
            dataList.Clear();

            var Serializer = new DataContractSerializer(typeof(List<double[]>));
            try
            {
                using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("DataList" + index))
                {
                    dataList.AddRange((List<double[]>)Serializer.ReadObject(stream));
                }
            }
            catch (Exception)
            {

                dataList.Clear();
            }
        }
    }
}
