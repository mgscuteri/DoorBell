using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DoorBell.Models;

namespace NetworkScanner.Helpers
{
    class PlaybackHelper
    {
        public List<string> playListMacs { get; set; }
        public bool isPlaying { get; set; }

        public PlaybackHelper()
        {
            playListMacs = new List<string> {};
        }

        public void addMacAddres(string macAddress)
        {
            playListMacs.Add(macAddress);
        }

        public void startPlayback()
        {
            //while playListMacs.length > 0
            while (playListMacs.Count > 0)
            {
                int playListCount = playListMacs.Count;

                while (playListCount == playListMacs.Count)
                {
                    string songUrl = playListMacs[0];
                    //play song, remove from playlist
                }
            }
        }

        public void playSong(string SongUrl, int videoDurationMiliseconds)
        {
            //This is untrusted data! be careful!
            Process songProcess;
            songProcess = Process.Start(SongUrl);
            //wait for duration of song
            var t2 = Task.Run(async delegate
            {
                await Task.Delay(videoDurationMiliseconds);  //video duration - start time 
                return 1;
            });
            t2.Wait();
            
            songProcess.Close();
        }

        public int getVideoDurationMiliseconds()
        {
            string url = @"http://gdata.youtube.com/feeds/api/videos/4TSJhIZmL0A?v=2&alt=jsonc&callback=youtubeFeedCallback&prettyprint=true";
            string duration = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                duration = reader.ReadToEnd();
            }
            int milisconds;
            //miliseconds = duration.toMiliseconds
            milisconds = 6000;
            return milisconds;
        }
    }
}
