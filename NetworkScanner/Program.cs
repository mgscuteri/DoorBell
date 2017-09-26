using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkScanner.Helpers;
using NetworkScanner.Models;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Xml;
using DoorBell.Models;
using System.Web;



namespace NetworkScanner
{
    class Program
    {
        public const int pollingIntervalMiliseconds = 750;
        public const int pingTimeOutMiliseconds = 500;
        public const int connectionTimeOutMinutes = 90;


        static void Main(string[] args)
        {
            List<ConnectedDevice> nonTimedOutDevices = new List<ConnectedDevice> { };
            List<ThemeSong> themeSongs = new List<ThemeSong>() { };
            PlaybackHelper playbackHelper = new PlaybackHelper();
            playbackHelper.isPlaying = false;


            while (1 == 1)
            {
                //Initialize network helper
                NetworkHelper netHelper = new NetworkHelper();
                
                netHelper.pingCounter = 253;  //Unless you've configured your router to assign limited ip's, Don't even mess with this. 
                //Ping all availble ip addresses to see which are active (Async + Threaded)
                netHelper.Ping_all(pingTimeOutMiliseconds);
                //Prepare to serialization helpers
                XmlSerializer connectedDeviceListSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ConnectedDevice>));
                XmlSerializer themeSongSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ThemeSong>));
                string connectedDevicesXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\connectedDevices.xml";
                string nonTimedOutDevicesXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\nonTimedOutDevices.xml";
                string themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\DoorBell\Data\ThemeSongs.xml";
                
                //Get list of macAddress song associates from webApi
                using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                {
                    themeSongs = (List<ThemeSong>) themeSongSerializer.Deserialize(reader);
                }
                //Remove Timed Out Connections 
                foreach (ConnectedDevice cd in nonTimedOutDevices)
                {
                    if (cd.isTimedOut(connectionTimeOutMinutes))
                    {
                        nonTimedOutDevices.Remove(cd);
                    }
                }
                //Wait for pings to finish -- ANY WORK not dependent on ping responses should go ABOVE here!  
                while (netHelper.pingCounter > 0)
                {
                    //wait for pings to finish 
                }
                //Create temporary list to avoid multithreaded shared memory issues
                List<ConnectedDevice> tempSuccessfulPings = netHelper.SuccessfullPings.ToList();
                //Add New connections to nonTimedOutDevices, and que up theme songs to be played.
                foreach (ConnectedDevice cd in tempSuccessfulPings)
                {
                    bool isNewConnection = !(nonTimedOutDevices.Any(item => item.macaddress == cd.macaddress));
                    bool existsInThemeSongs = (themeSongs.Any(item => item.macAddress == cd.macaddress));          ////THIS IS RETURNING TRUE WHEN IT SHOULD BE FALSE!!!!

                    if (isNewConnection && existsInThemeSongs) // && Matching macAddress exits in Data.ThemeSongs
                    {
                        //Limit playback to reasobable hours. ie not when you wake up in the morning. 
                        TimeSpan start = new TimeSpan(3, 30, 0); // 3:30 AM
                        TimeSpan end = new TimeSpan(10, 30, 0);  // 10:30 AM
                        TimeSpan now = DateTime.Now.TimeOfDay;
                        if (!((now > start) && (now < end)))
                        {
                            nonTimedOutDevices.Add(cd);
                            playbackHelper.playListMacs.Add(cd.macaddress); //send macaddress to playlist

                            if (playbackHelper.isPlaying == false)
                            {
                                playbackHelper.isPlaying = true;
                                Thread playbackThread = new Thread(playbackHelper.startPlayback);
                                playbackThread.Start();
                            }
                        }
                    }
                }
                
                System.IO.FileStream file = System.IO.File.Open(connectedDevicesXmlPath,FileMode.Truncate);
                connectedDeviceListSerializer.Serialize(file, netHelper.SuccessfullPings);
                file.Close();
                System.IO.FileStream file2 = System.IO.File.Open(nonTimedOutDevicesXmlPath, FileMode.Truncate);
                connectedDeviceListSerializer.Serialize(file2, nonTimedOutDevices);
                file2.Close();
                var t = Task.Run(async delegate
                {
                    await Task.Delay(pollingIntervalMiliseconds);
                    return 1;
                });
                t.Wait();
            }
        }
    }
}
