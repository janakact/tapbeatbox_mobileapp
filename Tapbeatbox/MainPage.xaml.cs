using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Tapbeatbox.TapLibrary;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using System.Runtime.Serialization;
using Windows.Storage.Streams;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tapbeatbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Lists
        private List<ToneSlot> listOfSlots = new List<ToneSlot>();
        private List<string> listOfToneNames = new List<string>();
        private List<int> listOfVolumes = new List<int>();

        Components.MediaManager mediaManager = new Components.MediaManager();

        //To be used when slot settings save
        int selectedSlotId = 0;

        //To save data
        Windows.Storage.ApplicationDataContainer localSettings;

        //App width and height to be used in the SetComponentSizes() funcion
        int AppWidth;
        int AppHeight;

        //Settings 
        int MasterVolume = 0;
        bool IsShareData = true;

        //To detect taps
        private DeviceListener deviceListener;
        private TapRecognizer tapRecognizer;
        private String playDetailTextValue;
        private bool playing = false;
        private bool trainingAll = false;

        //Progress value to be displayed in the training page
        private int TrainingProgressValue;
        private Components.TrainInfo trainInfo = new Components.TrainInfo();

        //The timewe is used to Loop to update interface
        DispatcherTimer dispatcherTimer;

       
        public MainPage()
        {

            this.InitializeComponent();
            this.DataContext = new ViewModel.HomePageViewModel();

            setup();
        }


        public void setup()
        {


            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            listOfToneNames = mediaManager.ToneNames;

            for (int i = Constant.minVolume; i <= Constant.maxVolume; i++)
            {
                listOfVolumes.Add(i);
            }

            System.Diagnostics.Debug.WriteLine("Starting the app.");



            //Load User Data
            dynamic a = localSettings.Values["SlotCount"];
            int slotCount = (a == null) ? 0 : (int)a;
            for (int i = 0; i < slotCount; i++)
            {
                ToneSlot s = LoadSlotItem(i);
                if (s != null) listOfSlots.Add(s);
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
                    listOfSlots.Add(new ToneSlot { ID = i, Name = "Slot" + i, Volume = Constant.defaultVolume, ToneName = listOfToneNames[0] });
                    SaveSlotItem(i);
                }
            }


            SlotList.ItemsSource = listOfSlots;


            //Get Window size
            SetComponentSizes();


            //Make the listener module
            deviceListener = DeviceListener.getInstance();
            deviceListener.OnTap += OnTap;
            DispatcherTimerSetup();

            tapRecognizer = new TapRecognizer(listOfSlots);
            playDetailTextValue = "Details of the Play";


            //Load Media
            mediaManager.loadMedia();
        }

        // Handles the Click event on the Button inside the Popup control and ------------------------------------------------------------------------
        // closes the Popup. 
        private void SaveSlotSettings(object sender, RoutedEventArgs e)
        {

            //Iterate and find the relevent slot
            int i = 0;
            foreach (var item in listOfSlots)
            {
                if (item.ID == selectedSlotId)
                {
                    item.Name = SlotSettings_Name.Text;
                    item.ToneName = SlotSettings_Tone.SelectedItem as string;
                    item.Volume = (int)SlotSettings_Volume.SelectedItem;
                    SaveSlotItem(i);
                    break;
                }
                i++;
            }

            // if the Popup is open, then close it 
            if (SlotSettings.IsOpen) { SlotSettings.IsOpen = false; }
        }


        // Handles the Click event on the Solts on the page and opens the Popup. 
        private void OpenSlotSettings(object sender, ItemClickEventArgs e)
        {
            ToneSlot s = e.ClickedItem as ToneSlot;

            SlotSettings_Name.Text = s.Name;
            SlotSettings_Tone.SelectedValue = s.ToneName;
            //foreach (var key in mediaManager.ToneNameIdMap.Keys)
            //{
            //    if (mediaManager.ToneNameIdMap[key] == s.ToneName)
            //        SlotSettings_Tone.SelectedValue = key;
            //}
            SlotSettings_Volume.SelectedItem = s.Volume;
            selectedSlotId = s.ID;

            // open the Popup if it isn't open already 
            if (!SlotSettings.IsOpen) { SlotSettings.IsOpen = true; }
        }

        //Training Page Related _______________________________________________________________________________________________________

        public void StartTraining(object sender, RoutedEventArgs e)
        {
            if (!TrainPage.IsOpen) { TrainPage.IsOpen = true; }

            trainingAll = false;
            TrainingProgressValue = 0;
            listOfSlots[selectedSlotId].trainingDataSet = new List<double[]>();

            //Start the traing thread;
            deviceListener.run();
        }
        public void CancelTraining(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (TrainPage.IsOpen && !trainingAll)
            {
                TrainPage.IsOpen = false;
                deviceListener.stop();
                deviceListener.getCurrentDataSet().slotId = selectedSlotId;
                var t = Task.Run(() =>
                {
                    NetworkClient.send(deviceListener.getCurrentDataSet());
                });

            }
        }

        public void TrainAll(object sender, RoutedEventArgs e)
        {
            trainInfo.Presentage = 0;
            trainingAll = true;
            TrainPage.IsOpen = true;
            var t = Task.Run(() =>
            {
                tapRecognizer.train(trainInfo);
            });
        }

        

      
            
        

        //Playing Page Related _______________________________________________________________________________________________________

        public void StartPlaying(object sender, RoutedEventArgs e)
        {
            if (!PlayPage.IsOpen)
            {
                PlayPage.IsOpen = true;
                if (!tapRecognizer.Trained)
                {
                    TrainAll(null, null);
                }
                deviceListener.run();
            }
        }
        public void CancelPlaying(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (PlayPage.IsOpen)
            {
                PlayPage.IsOpen = false;
                deviceListener.stop();
            }
        }

        //Settings Page Related _______________________________________________________________________________________________________
        public void OpenSettings(object sender, RoutedEventArgs e)
        {
            Settings_MasterVolume.SelectedItem = MasterVolume;
            Settings_IsShareData.IsChecked = IsShareData;
            if (!SettingsPage.IsOpen) { SettingsPage.IsOpen = true; }
        }
        public void CloseSettings(object sender, RoutedEventArgs e)
        {
            MasterVolume = (int)Settings_MasterVolume.SelectedItem;
            IsShareData = Settings_IsShareData.IsChecked.Value;

            localSettings.Values["MasterVolume"] = MasterVolume;
            localSettings.Values["IsShareData"] = IsShareData ? 1 : 0;
            // if the Popup is open, then close it 
            if (SettingsPage.IsOpen) { SettingsPage.IsOpen = false; }
        }



        //Load and Save Slot Items
        private void SaveSlotItem(int index)
        {


            if (index >= listOfSlots.Count) return;
            Windows.Storage.ApplicationDataCompositeValue composite =
     new Windows.Storage.ApplicationDataCompositeValue();

            ToneSlot slot = listOfSlots[index];
            composite["ID"] = slot.ID;
            composite["Name"] = slot.Name;
            composite["ToneName"] = slot.ToneName;
            composite["Volume"] = slot.Volume;

            localSettings.Values["Slot" + index] = composite;
            localSettings.Values["SlotCount"] = listOfSlots.Count;

            SaveTrainingData(slot.ID, slot.trainingDataSet);

        }

        //Load a slot item from the local storage
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
                slot.ToneName = listOfToneNames[0];
            }
            //slot.trainingDataSet = (List<double[]>)composite["DataSet"];
            LoadTrainingData(slot.ID, slot.trainingDataSet);
            return slot;
        }

        public async void SaveTrainingData(int index, List<double[]> dataList)
        {
            StorageFile userdetailsfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("DataList"+index, CreationCollisionOption.ReplaceExisting);

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

        public async void LoadTrainingData(int index, List<double[]> dataList)
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





        //Set component sizes to the required values when the app is starting
        private void SetComponentSizes()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = 1; //DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            Size size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            AppWidth = (int)size.Width;
            AppHeight = (int)size.Height;

            SlotSettingsBorder.Height = AppHeight;
            SlotSettingsBorder.Width = AppWidth;

            TrainPageBorder.Height = AppHeight;
            TrainPageBorder.Width = AppWidth;


            PlayPageBorder.Height = AppHeight;
            PlayPageBorder.Width = AppWidth;

            AppTitle.Text = AppWidth.ToString();
            // SlotSettings.Height = AppHeight;
        }

        private void OnTap(object sender, DeviceListener.TapEventArgs e)
        {
            if (playing)
            {
                double[] parms = e.parms;
                playDetailTextValue = "";
                for (int i = 0; i < parms.Length; i++)
                {
                    playDetailTextValue += parms[i] + "\n";
                }
                int recognizedValue = tapRecognizer.recognizeTheSlot(parms);
                playDetailTextValue += recognizedValue;

                //Play the tone
                mediaManager.Play(listOfSlots[recognizedValue].ToneName);

            }
            else
            {
                TrainingProgressValue++;
                listOfSlots[selectedSlotId].trainingDataSet.Add(e.parms);

                //Play the tone
                mediaManager.Play(listOfSlots[selectedSlotId].ToneName);
            }
        }

        //For timer
        public void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            //IsEnabled defaults to false

            dispatcherTimer.Start();
        }

        void dispatcherTimer_Tick(object sender, object e)
        {


            if (trainingAll)
            {
                TrainingPage_Progress.Value = trainInfo.Presentage;
                if (trainInfo.Presentage >= 1)
                    TrainPage.IsOpen = false;
            }
            else
            {
                TrainingPage_Progress.Value = (double)TrainingProgressValue / Constant.trainingTapCount;
                if (TrainingProgressValue >= 10)
                    CancelTraining(null, null);
            }

            PlayDetailsText.Text = playDetailTextValue;
            playing = PlayPage.IsOpen;
        }
        





    }
}
