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
                double SongDuration = ThemeSongs.FirstOrDefault(x => x.macAddress == playListMacs[0]).minutesToPlay;

                playListMacs.RemoveAt(0);
                playSong(SongURL, SongDuration);
            }
            
            isPlaying = false;
            Console.WriteLine("****-!!-**** Playback Complete. Terminating Playback Thread ****-!!-****");
        }

        public void playSong(string SongUrl, double minutesToPlayVideo)
        {
            Console.WriteLine("****----**** Luanching Browser ****----****");

            ProcessStartInfo procStartInfo;
            //ToDo: Treat this as untrusted data... 
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                procStartInfo = new ProcessStartInfo("cmd", $"/c start {SongUrl}"); // Works ok on windows
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                procStartInfo = new ProcessStartInfo("xdg-open", SongUrl);  // Works ok on linux
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                procStartInfo = new ProcessStartInfo("open", SongUrl); // Not tested
            }
            else 
            {
                throw(new Exception("Unsupported OS"));
            }

            Process proc = Process.Start(procStartInfo);
         
            Thread.Sleep((int)(minutesToPlayVideo * 60000));
            
            if (proc != null)
            {
                try
                {
                    Console.WriteLine("Killing Browser Window");
                    proc.Kill();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to close browser {ex.InnerException}");
                }
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
