using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DoorBell.Models;
using System.Xml;
using System.Xml.Serialization;


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
            // This code will allow the following block to run in a separate thread, instaed of async 
            //   int playListCount = playListMacs.Count;
            //    while (playListCount == playListMacs.Count)
            //    {
                    //Deserialize the themeSong/macaddress data
                    List<ThemeSong> themeSongs = new List<ThemeSong>() { };
                    XmlSerializer themeSongSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ThemeSong>));
                    string themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\DoorBell\Data\ThemeSongs.xml";
                    using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                    {
                        themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                    }
                    //get associated url of the current mac address
                    string thisYoutubeUrl = themeSongs.FirstOrDefault(x => x.macAddress == playListMacs[0]).songYoutubeUrl;
                    //get the duration of the song (async)
                    int videoDurationMiliseconds = 10;
                    var t3 = Task.Run(async delegate
                    {
                        videoDurationMiliseconds = getVideoDurationMiliseconds(thisYoutubeUrl);
                    });
                    t3.Wait();
                    //Play the song
                    playSong(thisYoutubeUrl, videoDurationMiliseconds);
                    //play song, remove from playlist
             //   }
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
            playListMacs.RemoveAt(0);
        }

        public int getVideoDurationMiliseconds(string url)
        {
            
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
            milisconds = 10000;
            return milisconds;
        }
    }
}
