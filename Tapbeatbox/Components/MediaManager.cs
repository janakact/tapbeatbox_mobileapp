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

        private List<MediaElement> mediaList = new List<MediaElement>();
        private Dictionary<string,int> _toneNameIdMap = new Dictionary<string, int> { { "Clap", 0 }, { "Foot", 1 }, { "Bell", 2 } };

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
            mediaList.Add(await LoadSoundFile("1.mp3"));
            mediaList.Add(await LoadSoundFile("2.mp3"));
            mediaList.Add(await LoadSoundFile("3.wav"));
        }


        private async Task<MediaElement> LoadSoundFile(string v)
        {
            MediaElement snd = new MediaElement();

            snd.AutoPlay = false;

            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync(v);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            snd.SetSource(stream, file.ContentType);
            return snd;
        }

        public async void Play(int index)
        {
            if (index >= mediaList.Count) return;
            await mediaList[index].Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaList[index].Play();
            });
        }

        public async void Play(string toneName)
        {
            int index = _toneNameIdMap[toneName];
            if (index>=0 && index<mediaList.Count)
            Play(index);
        }
    }
}
