using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Tapbeatbox.Components
{
    public class MediaManager
    {

        private List<MediaElement>[] mediaList = new List<MediaElement>[Constant.mediaCount];
        private Dictionary<string, int> _toneNameIdMap = new Dictionary<string, int>(); //{ { "Clap", 0 }, { "Foot", 1 }, { "Bell", 2 } };
        private List<string> fileNames = new List<string>();

        int playIndex = 0;
        public MediaManager()
        {
            string[] types = {"Clap","Clash", "Hihat", "Kick", "LectureHall", "Shaker", "Shotgun", "Snare" };
            int[] counts = { 2, 2, 2, 7, 2, 3, 1, 12 };

            int tot = 0;
            for (int i = 0; i < types.Length; i++)
            {
                for (int j = 1; j <= counts[i]; j++)
                {
                    fileNames.Add(types[i]+j+".wav");
                    _toneNameIdMap.Add(types[i] + j, tot);
                    tot++;
                }
            }
            //fileNames.Add("1.mp3");
            //fileNames.Add("1.mp3");
            //_toneNameIdMap.Add("Clap1", 0);
            //_toneNameIdMap.Add("Clap2", 1);

        }

        public Dictionary<string,int> ToneNameIdMap
        {
            get { return _toneNameIdMap; }
        }

        public List<string> ToneNames
        {
            get { return new List<string>(_toneNameIdMap.Keys); }
        }
        
       

        public async void loadMedia()
        {
            for (int m = 0; m < Constant.mediaCount; m++)
            {
                mediaList[m] = new List<MediaElement>();
                for (int i = 0; i < fileNames.Count; i++)
                {
                    mediaList[m].Add(await LoadSoundFile(fileNames[i]));
                }
            }
            //mediaList.Add(await LoadSoundFile("1.mp3"));
            //mediaList.Add(await LoadSoundFile("2.mp3"));
            //mediaList.Add(await LoadSoundFile("3.wav"));
        }


        private async Task<MediaElement> LoadSoundFile(string v)
        {
            MediaElement snd = new MediaElement();


            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets\\media");
            StorageFile file = await folder.GetFileAsync(v);
            var stream = await file.OpenAsync(FileAccessMode.Read);
                        
            snd = new MediaElement();
            snd.AutoPlay = false;
            snd.SetSource(stream, file.ContentType);
            
            return snd;
        }

        public async void Play(int index)
        {
            if (index >= mediaList[0].Count) return;
            playIndex=(playIndex+1)%Constant.mediaCount;
            await mediaList[playIndex][index].Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaList[playIndex][index].Play();
                mediaList[playIndex][index].Stop();
            });
            
        }

        public async void Play(string toneName)
        {
            int index = 0;
            try
            {
                index = _toneNameIdMap[toneName];
            }
            catch
            {

            }
            if (index >= 0 && index < mediaList[0].Count)
                Play(index);
        }
    }
}
