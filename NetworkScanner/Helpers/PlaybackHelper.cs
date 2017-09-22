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

        public void playSong(string youTubeUrl)
        {
            //This is untrusted data! be careful!
            Process songProcess;
            songProcess = Process.Start(youTubeUrl);
            //wait for duration of song
            songProcess.Close();
        }

        public string getVideoDuration()
        {
            string url = @"";
            string duration = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                duration = reader.ReadToEnd();
            }

            return duration;
        }
    }
}
