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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tapbeatbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<Slot> listOfSlots = new List<Slot>();
        private List<string> listOfToneNames = new List<string>();
        private List<int> listOfVolumes= new List<int>();

        //To be used when slot settings save
        int selectedSlotId = 0;

        //To save data
        Windows.Storage.ApplicationDataContainer localSettings;

        int AppWidth;
        int AppHeight;

        //Settings 
        int MasterVolume = 0;
        bool IsShareData = true;

        public MainPage()
        {
            this.InitializeComponent();

            localSettings  = Windows.Storage.ApplicationData.Current.LocalSettings;

      
            listOfToneNames.Add("Clap");
            listOfToneNames.Add("Foot");

            for(int i=Constant.minVolume; i<= Constant.maxVolume; i++)
            {
                listOfVolumes.Add(i);
            }

            

            //Load User Data
            dynamic a = localSettings.Values["SlotCount"];
            int slotCount = (a == null) ? 0 : (int)a;
            for (int i=0; i< slotCount; i++)
            {
                Slot s = LoadSlotItem(i);
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
                    listOfSlots.Add(new Slot { ID = i, Name = "Slot" + i, Volume = Constant.defaultVolume, ToneName = listOfToneNames[0] });
                    SaveSlotItem(i);
                }
            }


            SlotList.ItemsSource = listOfSlots;


            //Get Window size
            SetComponentSizes();
            


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
                    item.ToneName = SlotSettings_Tone.SelectedItem as String;
                    item.Volume = (int)SlotSettings_Volume.SelectedItem ;
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
            Slot s = e.ClickedItem as Slot;

            SlotSettings_Name.Text = s.Name;
            SlotSettings_Tone.SelectedItem = s.ToneName;
            SlotSettings_Volume.SelectedItem = s.Volume;
            selectedSlotId = s.ID;

            // open the Popup if it isn't open already 
            if (!SlotSettings.IsOpen) { SlotSettings.IsOpen = true; }
        }

        //Training Page Related _______________________________________________________________________________________________________

        public void StartTraining(object sender, RoutedEventArgs e)
        {
            if (!TrainPage.IsOpen) { TrainPage.IsOpen = true; }
        }
        public void CancelTraining(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (TrainPage.IsOpen) { TrainPage.IsOpen = false; }
        }


        //Playing Page Related _______________________________________________________________________________________________________

        public void StartPlaying(object sender, RoutedEventArgs e)
        {
            if (!PlayPage.IsOpen) { PlayPage.IsOpen = true; }
        }
        public void CancelPlaying(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (PlayPage.IsOpen) { PlayPage.IsOpen = false; }
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
            MasterVolume = (int) Settings_MasterVolume.SelectedItem;
            IsShareData = Settings_IsShareData.IsChecked.Value;

            localSettings.Values["MasterVolume"] = MasterVolume;
            localSettings.Values["IsShareData"] = IsShareData?1:0;
            // if the Popup is open, then close it 
            if (SettingsPage.IsOpen) { SettingsPage.IsOpen = false; }
        }



        //Load and Save Slot Items
        private void SaveSlotItem(int index)
        {
            if (index >= listOfSlots.Count) return;
            Windows.Storage.ApplicationDataCompositeValue composite =
     new Windows.Storage.ApplicationDataCompositeValue();

            Slot slot = listOfSlots[index];
            composite["ID"] = slot.ID;
            composite["Name"] = slot.Name;
            composite["ToneName"] = slot.ToneName;
            composite["Volume"] = slot.Volume;

            localSettings.Values["Slot" + index] = composite;
            localSettings.Values["SlotCount"] = listOfSlots.Count;

        }
        private Slot LoadSlotItem(int index)
        {
            Slot slot = new Slot();
            Windows.Storage.ApplicationDataCompositeValue composite = localSettings.Values["Slot" + index] as  Windows.Storage.ApplicationDataCompositeValue;

            if (composite == null) return null;

            slot.ID = (int)composite["ID"];
            slot.Name = (string)composite["Name"];
            slot.Volume = (int)composite["Volume"];
            slot.ToneName =  (string)composite["ToneName"] ;

            return slot;
        }


        



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


    }
}
