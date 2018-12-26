using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NetworkScanner.Models;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetworkScanner.Helpers
{
    public class PlaybackHelper
    {
        private List<string> playListMacs;
        private bool isPlaying;
        private List<ThemeSong> ThemeSongs;
        private XmlSerializer themeSongSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ThemeSong>));
        private string themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\Data\ThemeSongs.xml";

        public PlaybackHelper()
        {
            playListMacs = new List<string> {};
            isPlaying = false;
        }

        public void addMacAddres(string macAddress)
        {
            lock(playListMacs)
            {
                Console.WriteLine($"****----** Adding {macAddress} to playlist **----****");
                playListMacs.Add(macAddress);
            }
            
            if (isPlaying == false)
            {                
                isPlaying = true;
                Thread playbackThread = new Thread(startPlayback);
                playbackThread.Start();
            }
        }

        public void startPlayback()
        {
            Console.WriteLine($"****-!!-**** Creating Playback Thread ****-!!-****");

            while (playListMacs.Count > 0)
            {
                using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                {
                    ThemeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                }
                string SongURL = ThemeSongs.FirstOrDefault(x => x.macAddress == playListMacs[0]).songYoutubeUrl;
                int SongDuration = ThemeSongs.FirstOrDefault(x => x.macAddress == playListMacs[0]).duration;

                playListMacs.RemoveAt(0);
                playSong(SongURL, SongDuration);
            }
            
            isPlaying = false;
            Console.WriteLine("****-!!-**** Playback Complete. Terminating Playback Thread ****-!!-****");
        }

        public void playSong(string SongUrl, int duration)
        {
            Console.WriteLine("****----**** Playing Song ****----****");
            OpenBrowserToUrl(SongUrl);
            Console.WriteLine("****----**** Playback thread sleeping ****----****");
            Thread.Sleep(duration);
            Console.WriteLine("****----**** Playback thread waking ****----****");
            CloseBrowser();
        }

        public static void OpenBrowserToUrl(string url)
        {
            Console.WriteLine("****----**** Luanching Browser ****----****");

            ProcessStartInfo procStartInfo;
            //ToDo: Treat this as untrusted data... 
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                procStartInfo = new ProcessStartInfo("cmd", $"/c start {url}"); // Works ok on windows
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                procStartInfo = new ProcessStartInfo("xdg-open", url);  // Works ok on linux
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                procStartInfo = new ProcessStartInfo("open", url); // Not tested
            }
            else
            {
                throw (new Exception("Unsupported OS"));
            }

            Process proc = Process.Start(procStartInfo);
        }

        public static void CloseBrowser()
        {
            Console.WriteLine("****----**** Closing Browser ****----****");
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process[] chromeInstances = Process.GetProcessesByName("chrome");

                    foreach (Process p in chromeInstances)
                    {
                        p.Kill();
                    }                        
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    //procCloseInfo = new ProcessStartInfo("xdg-open");  // Need to implement for linux
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    //procCloseInfo = new ProcessStartInfo("open"); // Need to implement for mac
                }
                else
                {
                    throw (new Exception("Unsupported OS"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to close browser {ex.InnerException}");
            }
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
