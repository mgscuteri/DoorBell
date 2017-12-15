using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DoorBell.Models;
using NetworkScanner.Data;
using NetworkScanner.Models;
using NetworkScanner;

namespace NetworkScanner.Helpers
{
    public class NetworkHelper
    {
        //program constants        
        public const int pingTimeOutMiliseconds = 200; //500
        public const int connectionTimeOutMinutes = 90;
        public const bool testMode = false;
        public string connectedDevicesXmlPath;
        public string nonTimedOutDevicesXmlPath;
        public string masterDeviceListXmlPath;
        public string themeSongsXmlPath;
        public XmlSerializer connectedDeviceListSerializer;
        public XmlSerializer themeSongSerializer;
        //program variables
        public List<ConnectedDevice> nonTimedOutDevices ;
        public List<ConnectedDevice> masterDeviceList ;
        public List<ThemeSong> themeSongs;
        //program helpers
        private PlaybackHelper playbackHelper;

        public NetworkHelper()
        {
            nonTimedOutDevices = new List<ConnectedDevice> { };
            masterDeviceList = new List<ConnectedDevice> { };
            themeSongs = new List<ThemeSong> { };
            connectedDevicesXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\connectedDevices.xml";
            nonTimedOutDevicesXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\nonTimedOutDevices.xml";
            masterDeviceListXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\data\masterDeviceList.xml";
            themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\DoorBell\Data\ThemeSongs.xml";

            connectedDeviceListSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
            themeSongSerializer = new XmlSerializer(typeof(List<ThemeSong>));

            playbackHelper = new PlaybackHelper();
            playbackHelper.isPlaying = false;
        }

        
        public void ProcessPlayback(ConnectedDevice cd)
        {
            //Limit playback to reasobable hours. ie not when you wake up in the morning. 
            TimeSpan start = new TimeSpan(3, 30, 0); // 3:30 AM
            TimeSpan end = new TimeSpan(10, 30, 0);  // 10:30 AM
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (now > start) //its after 3:30 am
            {
                if (now > end) //its after 3:30 am and after 10:30 am
                {
                    lock (playbackHelper.playListMacs)
                    {
                        //Queue song for playback
                        playbackHelper.playListMacs.Add(cd.macaddress);     
                    }
                    Console.WriteLine("**** Added Mac Address to playback list: " + cd.macaddress + " (Name: " + cd.hostname + ")");
                    Console.WriteLine("**** Is the playback helper currently playing? " + playbackHelper.isPlaying.ToString());

                    if (playbackHelper.isPlaying == false)
                    {
                        Console.WriteLine("****STARTING PLAYBACK****");
                        playbackHelper.isPlaying = true;
                        Thread playbackThread = new Thread(playbackHelper.startPlayback);
                        playbackThread.Start();
                    }
                }
                else //its after 3:30am, but before 10:30 am
                {
                    Console.WriteLine("************ Its early, I'm gonna take a nap... ");
                    Thread.Sleep(3600000 * 7);
                    Console.WriteLine("Waking Up now!");
                }
            }
        }

        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            try
            {
                if (e.Reply != null && e.Reply.Status == IPStatus.Success)
                {
                    //Parse the response into object
                    string ip = (string)e.UserState;
                    string hostname = GetHostName(ip);
                    string macaddres = GetMacAddress(ip);
                    string[] arr = new string[3];
                    
                    arr[0] = ip;
                    arr[1] = hostname;
                    arr[2] = macaddres;
                    
                    ConnectedDevice pingResults = new ConnectedDevice
                    {
                        hostname = hostname,
                        ip = ip,
                        macaddress = macaddres,
                        connectDateTime = DateTime.UtcNow,
                        isNewConnection = true
                    };

                    // --> Update the Master Device List (All devices that have ever connected)
                    if (masterDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                    {
                        //This macAddress has connected before. Lets update its IP address in the master ip/mac lookup table 
                        ConnectedDevice knownDevice = masterDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                        if (knownDevice.ip != pingResults.ip)
                        {
                            Console.WriteLine("...Known device (" + pingResults.macaddress + ") has a new ip address. Updating IP:" + knownDevice.ip);
                            lock (nonTimedOutDevices)
                            {
                                knownDevice.ip = pingResults.ip;
                            }
                        }
                        else
                        {
                            Console.WriteLine("...Known device (" + pingResults.macaddress + ") detected.");
                        }
                    }
                    else
                    {
                        //This macAddress has not connected before. Add it to the masterDeviceList
                        Console.WriteLine("2.0) Unkown device detected. Adding to master device list.:" + pingResults.macaddress);
                        lock (nonTimedOutDevices)
                        {
                            masterDeviceList.Add(pingResults);
                        }
                    }

                    // --> Upodate the NonTimedOutDevice list (Devices defined by program to be currently "connected")
                    if (nonTimedOutDevices.Any(x => x.macaddress == pingResults.macaddress))
                    {
                        //Device is currently "connected" (has not timed out). Need to update its timestamp.
                        Console.WriteLine("...A connected device has reconnected. Updating its timeStamp:" + pingResults.macaddress + ":" + DateTime.UtcNow.ToString());
                        ConnectedDevice reconnectedDevice = nonTimedOutDevices.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                        lock (nonTimedOutDevices)
                        {
                            reconnectedDevice.connectDateTime = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        //This is a new "connection". Process it. 
                        Console.WriteLine("3) *A device has connected- Mac:" + pingResults.macaddress + "  - Name: " + pingResults.hostname);
                        lock (nonTimedOutDevices)
                        {
                            nonTimedOutDevices.Add(pingResults);
                        }

                        if (themeSongs.Any(x => x.macAddress == pingResults.macaddress))
                        {
                            //Device has an associated theme song
                            Console.WriteLine("+++ Device has an associated theme song. Considering for playback.");
                            ProcessPlayback(pingResults);
                        }
                        else
                        {
                            Console.WriteLine("Device does NOT have an associated theme song.");
                            // Do nothing
                        }
                    }
                }

            }
            catch (Exception)
            {
                //
            }
        }



        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate ()
                {
                    try
                    {
                        Ping ping = new Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {
                        // Do nothing and let it try again until the attempts are exausted.
                        // Exceptions are thrown for normal ping failurs like address lookup
                        // failed.  For this reason we are supressing errors.
                    }
                }).Start();
            }
        }

        public void Ping_all()
        {            
            Console.WriteLine("    1.0) Entering Ping All function - Deserializing data into memory");
            XmlSerializer connectedDeviceListSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
            XmlSerializer themeSongSerializer = new XmlSerializer(typeof(List<ThemeSong>));
            
            
            using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
            {
                themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
            }
            //Get list of nonTimedOutDevices -- Test mode causes all devices to be seen as new
            using (XmlReader reader = XmlReader.Create(nonTimedOutDevicesXmlPath))
            {
                nonTimedOutDevices = (List<ConnectedDevice>)connectedDeviceListSerializer.Deserialize(reader);
            }
            //Get master list of devices
            using (XmlReader reader = XmlReader.Create(masterDeviceListXmlPath))
            {
                masterDeviceList = (List<ConnectedDevice>)connectedDeviceListSerializer.Deserialize(reader);
            }

            //Extracting and pinging all other ip's.
            string gate_ip = NetworkHelper.NetworkGateway();
            string[] array = gate_ip.Split('.');

            Console.WriteLine("    1.1) Beggining Pings.");

            //parallel for  response collection
            for (int i = 2; i <= 255; i++)
            {

                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                //time in milliseconds           
                Ping(ping_var, 1, pingTimeOutMiliseconds);
            }
            Console.WriteLine("    1.2) A Ping All has completed.");


            //Check for timed out devices
            foreach (ConnectedDevice cd in nonTimedOutDevices.Reverse<ConnectedDevice>())
            {
                if (cd.isTimedOut(NetworkHelper.connectionTimeOutMinutes))
                {
                    nonTimedOutDevices.Remove(cd);
                    Console.WriteLine("DEVICE TIMED OUT - REMOVING: " + cd.macaddress + " from connected(non timed out) list");
                }
            }

            System.IO.FileStream nonTimedOoutDevicesFile = System.IO.File.Open(nonTimedOutDevicesXmlPath, FileMode.Truncate);
            connectedDeviceListSerializer.Serialize(nonTimedOoutDevicesFile, nonTimedOutDevices);
            nonTimedOoutDevicesFile.Close();
            System.IO.FileStream masterDeviceListFile = System.IO.File.Open(masterDeviceListXmlPath, FileMode.Truncate);
            connectedDeviceListSerializer.Serialize(masterDeviceListFile, masterDeviceList);
            masterDeviceListFile.Close();            
        }

        
        /// Helper functions \/

        static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        if (d.Address.ToString() != "::")
                            ip = d.Address.ToString();
                    }
                }
            }

            return ip;
        }


        public string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                // MessageBox.Show(e.Message.ToString());
            }

            return null;
        }


        //Get MAC address
        public string GetMacAddress(string ipAddress)
        {
            string macAddress = String.Empty;
            Process Process = new Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "OWN Machine";
            }
        }
    }
}