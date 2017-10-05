using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NetworkScanner.Data;


namespace NetworkScanner
{
    class Program
    {
        //program variables
        public const int pingTimeOutMiliseconds = 200; //500
        public const int connectionTimeOutMinutes = 90;
        public const bool testMode = false;

        public List<ConnectedDevice> successfullNetworkPings = new List<ConnectedDevice>() { };

        static void Main(string[] args)
        {
            List<ConnectedDevice> nonTimedOutDevices = new List<ConnectedDevice> { };
            List<ConnectedDevice> masterDeviceList = new List<ConnectedDevice> { };
            List<ThemeSong> themeSongs = new List<ThemeSong>() { };
            PlaybackHelper playbackHelper = new PlaybackHelper();
            playbackHelper.isPlaying = false;
            

            //Prepare serialization helpers
            XmlSerializer connectedDeviceListSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ConnectedDevice>));
            XmlSerializer themeSongSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ThemeSong>));
            string connectedDevicesXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\connectedDevices.xml";
            string nonTimedOutDevicesXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\nonTimedOutDevices.xml";
            string masterDeviceListXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\masterDeviceList.xml";
            string themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\DoorBell\Data\ThemeSongs.xml";


            while (1 == 1)
            {
                try
                {
                    //Initialize network helper
                    NetworkHelper netHelper = new NetworkHelper();
                    netHelper.programState = ProgramState.DoneProcessingPingResponses;

                    netHelper.pingCounter = 253;  //Unless you've configured your router to assign limited ip's, Don't even mess with this. 
                                                  //Ping all availble ip addresses to see which are active (Async + Threaded)

                    //Don't start next round of pings until we've finished acting on the previous round 
                    while(netHelper.programState != ProgramState.DoneProcessingPingResponses)
                    {
                        //Wait
                    }
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    netHelper.programState = ProgramState.PingAllStarted;
                    Console.WriteLine("1) Beggining Pings");
                    netHelper.Ping_all(pingTimeOutMiliseconds);                    
                  
                    //Get list of macAddress song associates from webApi
                    using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                    {
                        themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                    }
                    //Get list of nonTimedOutDevices -- Test mode causes all devices to be seen as new
                    if (!testMode)
                    {
                        while(netHelper.programState == ProgramState.removingTimedOutConnections)
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

                    
                    //wait for pings to finish
                    while (netHelper.programState != ProgramState.PingAllCompleted)
                    {
                        //wait
                    }
                    netHelper.programState = ProgramState.ProcessingPingResponses;
                    //Remove Timed Out Connections 
                    foreach (ConnectedDevice cd in nonTimedOutDevices.Reverse<ConnectedDevice>())
                    {
                        if (cd.isTimedOut(connectionTimeOutMinutes))
                        {
                            nonTimedOutDevices.Remove(cd);
                            Console.WriteLine("DEVICE TIMED OUT - REMOVING: " + cd.macaddress + " from connected(non timed out) list");
                        }
                    }
                    Console.WriteLine("3) Entering loop with [" + netHelper.SuccessfullPings.Count.ToString() + "] responses. Locking Pings");
                    lock(netHelper.SuccessfullPings)
                    { 
                        foreach (ConnectedDevice cd in netHelper.SuccessfullPings)
                        {
                            
                            bool ipAddressExistsInMasterList = (masterDeviceList.Any(item => item.ip == cd.ip));
                            bool isNewNonTimedOutConnection = !(nonTimedOutDevices.Any(item => item.macaddress == cd.macaddress));
                            bool existsInThemeSongs = (themeSongs.Any(item => item.macAddress == cd.macaddress));

                            //If its not a NON-timed out conection, (device has connected within 90 minutes and is)it is still connected. update the connection time.
                            if(!isNewNonTimedOutConnection)
                            {
                                Console.WriteLine(cd.macaddress + " Connected. Device is not timed out. Updating connect time:" + DateTime.UtcNow.ToString());
                                ConnectedDevice nonTimedOutDevice = nonTimedOutDevices.Where(x => x.macaddress == cd.macaddress).FirstOrDefault();
                                nonTimedOutDevice.connectDateTime = DateTime.UtcNow;
                            }
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
                                    Console.WriteLine("*****FOUND A RECOGNIZED CONNECTION: " + cd.macaddress);

                                    if (playbackHelper.isPlaying == false)
                                    {
                                        Console.WriteLine("*****STARTING PLAYBACK");
                                        playbackHelper.isPlaying = true;
                                        Thread playbackThread = new Thread(playbackHelper.startPlayback);
                                        playbackThread.Start();
                                    }
                                }
                            }
                        }
                        netHelper.programState = ProgramState.DoneProcessingPingResponses;
                        Console.WriteLine("4) Done proccessing pings. Ready to launch new round of pings.");
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
                    Console.WriteLine("~Miliseconds elapsed this iteration: [" + timer.ElapsedMilliseconds.ToString() + "]");
                }
                catch(Exception ex)
                {
                    //something went wrong
                }
             }
        }
    }
}
