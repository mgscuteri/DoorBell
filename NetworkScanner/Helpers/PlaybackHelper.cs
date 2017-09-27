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
using Microsoft.Win32;


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
                    double minutesToPlayThisVideo = themeSongs.FirstOrDefault(x => x.macAddress == playListMacs[0]).minutesToPlay;
                    //get the duration of the song (async)                    

                    /* Making an api call to youtube to get the duration of a video is slightly more difficult than I expected.. uncomment this if you manage to fix the getVideoDuration() method
                    double videoDurationMinutes;
                    var t3 = Task.Run(async delegate
                    {
                        videoDurationMinutes = getVideoDurationMiliseconds(thisYoutubeUrl);
                    });    
                    t3.Wait();
                    */

                    //Play the song
                    playSong(thisYoutubeUrl, minutesToPlayThisVideo);
                    //play song, remove from playlist
             //   }
            }
        }

        public void playSong(string SongUrl, double minutesToPlayVideo)
        {
            //This is untrusted data! be careful!
            //Get the default browser
            string browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command");

                //trim off quotes
                if (key != null)
                {
                    browser = key.GetValue(null).ToString().ToLower().Trim(new[] { '"' });
                }
                if (!browser.EndsWith("exe"))
                {
                    //get rid of everything after the ".exe"
                    browser = browser.Substring(0, browser.LastIndexOf(".exe", StringComparison.InvariantCultureIgnoreCase) + 4);
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }

            //Launch the default browser
            Process proc = Process.Start(browser, SongUrl);
         
            //wait for duration of song
            var t2 = Task.Run(async delegate
            {
                await Task.Delay((int)(minutesToPlayVideo * 60000));  //video duration - start time 
                return 1;
            });
            t2.Wait();
            //Kill the browser tab
            if (proc != null)
            {
                proc.Kill();
            }
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
            milisconds = 10000; //BUG not fully implemented 
            return milisconds;
        }
    }
}
