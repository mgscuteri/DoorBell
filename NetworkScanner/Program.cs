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
        public const int pollingIntervalMiliseconds = 1000; //750
        public const int pingTimeOutMiliseconds = 700; //500
        public const int connectionTimeOutMinutes = 90;


        static void Main(string[] args)
        {
            List<ConnectedDevice> nonTimedOutDevices = new List<ConnectedDevice> { };
            List<ConnectedDevice> masterDeviceList = new List<ConnectedDevice> { };
            List<ThemeSong> themeSongs = new List<ThemeSong>() { };
            PlaybackHelper playbackHelper = new PlaybackHelper();
            playbackHelper.isPlaying = false;
            bool testMode = true;

            while (1 == 1)
            {
                try
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
                    string masterDeviceListXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\masterDeviceList.xml";
                    string themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\DoorBell\Data\ThemeSongs.xml";

                    //Get list of macAddress song associates from webApi
                    using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                    {
                        themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                    }
                    //Get list of nonTimedOutDevices -- Test mode causes all devices to be seen as new
                    if (!testMode)
                    {
                        using (XmlReader reader = XmlReader.Create(nonTimedOutDevicesXmlPath))
                        {
                            nonTimedOutDevices = (List<ConnectedDevice>)connectedDeviceListSerializer.Deserialize(reader);
                        }
                    }
                    //Get master list of devices
                    using (XmlReader reader = XmlReader.Create(masterDeviceListXmlPath))
                    {
                        masterDeviceList = (List<ConnectedDevice>)connectedDeviceListSerializer.Deserialize(reader);
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
                        bool ipAddressExistsInMasterList = (masterDeviceList.Any(item => item.ip == cd.ip));
                        bool isNewNonTimedOutConnection = !(nonTimedOutDevices.Any(item => item.macaddress == cd.macaddress));
                        bool existsInThemeSongs = (themeSongs.Any(item => item.macAddress == cd.macaddress));

                        // Add device to master macAddress store
                        if (ipAddressExistsInMasterList)
                        {
                            string macAddressForCurrentIp = masterDeviceList.Where(item => item.ip == cd.ip).FirstOrDefault().macaddress;

                            if (cd.macaddress != macAddressForCurrentIp)
                            {
                                //This ip has already been asigned before!
                                ConnectedDevice ConnectionToDelete = masterDeviceList.Where(item => item.ip == cd.ip).FirstOrDefault();
                                masterDeviceList.Remove(ConnectionToDelete);
                                masterDeviceList.Add(cd);
                            }
                        }
                        else
                        {
                            masterDeviceList.Add(cd);
                        }

                        if (isNewNonTimedOutConnection && existsInThemeSongs)
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

                    System.IO.FileStream connectedDevicesFile = System.IO.File.Open(connectedDevicesXmlPath, FileMode.Truncate);
                    connectedDeviceListSerializer.Serialize(connectedDevicesFile, netHelper.SuccessfullPings);
                    connectedDevicesFile.Close();
                    System.IO.FileStream nonTimedOoutDevicesFile = System.IO.File.Open(nonTimedOutDevicesXmlPath, FileMode.Truncate);
                    connectedDeviceListSerializer.Serialize(nonTimedOoutDevicesFile, nonTimedOutDevices);
                    nonTimedOoutDevicesFile.Close();
                    System.IO.FileStream masterDeviceListFile = System.IO.File.Open(masterDeviceListXmlPath, FileMode.Truncate);
                    connectedDeviceListSerializer.Serialize(masterDeviceListFile, masterDeviceList);
                    masterDeviceListFile.Close();

                    var t = Task.Run(async delegate
                    {
                        await Task.Delay(pollingIntervalMiliseconds);
                        return 1;
                    });
                    t.Wait();
                }
                catch
                {
                    //something went wrong
                }
             }
        }
    }
}
