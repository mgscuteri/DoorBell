using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using NetworkScanner.Models;
using System.Threading.Tasks;

namespace NetworkScanner.Helpers
{
    public class NetworkHelper
    {
        public const int connectionTimeOutMinutes = 720; //12 hours 
        public const bool verboseLogging = true;
        public const bool extraVerboseLogging = false;
        public int pingTimeOutMiliseconds = 4000;
        public string ConnectedDeviceListXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\data\ConnectedDevices.xml";
        public string MasterDeviceListXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\data\masterDeviceList.xml";
        public string themeSongsXmlPath = Directory.GetParent((Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName)) + @"\Data\ThemeSongs.xml";
        public XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
        public XmlSerializer themeSongSerializer = new XmlSerializer(typeof(List<ThemeSong>));
        public List<ConnectedDevice> ConnectedDeviceList ;
        public List<ConnectedDevice> masterDeviceList ;
        public List<ThemeSong> themeSongs;
        private PlaybackHelper playbackHelper;

        public NetworkHelper(int pingAllPollInterval)
        {
            ConnectedDeviceList = new List<ConnectedDevice> { };
            masterDeviceList = new List<ConnectedDevice> { };
            themeSongs = new List<ThemeSong> { };
            pingTimeOutMiliseconds = pingAllPollInterval + 2000;
            playbackHelper = new PlaybackHelper();
        }

        public void Ping_all()
        {
            Console.WriteLine("Pinging all possible LAN addresses");

            string gate_ip = NetworkHelper.NetworkGateway();
            
            if(gate_ip == null)
            {
                Console.WriteLine("Network Gateway not found!");
                throw (new Exception("Network Gateway not Found."));
            }

            string[] array = gate_ip.Split('.');

            Parallel.For(2, 255, i =>
            {
                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                //time in milliseconds           
                Ping(ping_var, 1, pingTimeOutMiliseconds);
            });
        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    if (extraVerboseLogging) { Console.WriteLine($"Pinging {host}"); }

                    Ping ping = new Ping();
                    ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                    ping.SendAsync(host, timeout, host);
                }
                catch
                {
                    Console.WriteLine($"! ! - - * * Ping execution failed. Host: {host}");
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
                        userName = "unknown",
                        ip = ip,
                        macaddress = macaddres,
                        connectDateTime = DateTime.UtcNow,
                    };
                    if (verboseLogging) { Console.WriteLine($"Processing Response From: {pingResults.macaddress}"); }

                    if (verboseLogging) { Console.WriteLine($"Checking: {pingResults.macaddress} against master device list"); }
                    if (masterDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                    {
                        //This macAddress has connected before. Lets update its IP address in the master ip/mac lookup table 
                        ConnectedDevice knownDevice = masterDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                        if (knownDevice != null && knownDevice.ip != pingResults.ip)
                        {
                            Console.WriteLine("- - Known device (" + pingResults.macaddress + ") has a new ip address. Updating IP:" + knownDevice.ip);
                            knownDevice.ip = pingResults.ip;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"- - * * New device detected. Adding to master device list - Host Name: {pingResults.hostname} - Mac Address: {pingResults.macaddress}");
                        masterDeviceList.Add(pingResults);
                    }

                    if (verboseLogging) { Console.WriteLine($"Checking: {pingResults.macaddress} against connected device list"); }
                    if (ConnectedDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                    {
                        //A connected device has reconnected. Updating its timestamp. 
                        ConnectedDevice reconnectedDevice = ConnectedDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                        reconnectedDevice.connectDateTime = DateTime.UtcNow;
                    }
                    else if (themeSongs.Any(x => x.macAddress == pingResults.macaddress))
                    {
                        //This is a new "connection". Process it. 
                        Console.WriteLine($"* * * * - -- - * * * * New device detected. Adding to master device list - UserName: {themeSongs.Where(x => x.macAddress == pingResults.macaddress).FirstOrDefault()}");
                            
                        ConnectedDeviceList.Add(pingResults);
                        ProcessPlayback(pingResults);
                    }

                    if (verboseLogging) { Console.WriteLine($"Done processing {pingResults.macaddress}"); }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"! ! ! ! - - - - * * * * PingCompleted() has encountered an error: {ex.InnerException}");
            }
        }

        public void DeserializeDataStores()
        {
            lock (masterDeviceList) lock (ConnectedDeviceList)
            {
                Console.WriteLine("- - >< Deserializing Data Stores");
                using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                {
                    themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                }
                //Get list of nonTimedOutDevices -- Test mode causes all devices to be seen as new
                using (XmlReader reader = XmlReader.Create(ConnectedDeviceListXmlPath))
                {
                    ConnectedDeviceList = (List<ConnectedDevice>)connectedDeviceSerializer.Deserialize(reader);
                }
                //Get master list of devices
                using (XmlReader reader = XmlReader.Create(MasterDeviceListXmlPath))
                {
                    masterDeviceList = (List<ConnectedDevice>)connectedDeviceSerializer.Deserialize(reader);
                }
            }
        }

        public void SerializeDataStores()
        {
            lock (masterDeviceList) lock (ConnectedDeviceList)
                {
                    Console.WriteLine("- - <> Serializing Data Stores");
                    try
                    {
                        FileStream connectedDeviceListXmlFile = File.Open(ConnectedDeviceListXmlPath, FileMode.Truncate);
                        connectedDeviceSerializer.Serialize(connectedDeviceListXmlFile, ConnectedDeviceList);
                        connectedDeviceListXmlFile.Close();

                        FileStream masterDeviceListXmFile = File.Open(MasterDeviceListXmlPath, FileMode.Truncate);
                        connectedDeviceSerializer.Serialize(masterDeviceListXmFile, masterDeviceList);
                        masterDeviceListXmFile.Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Serialization Failure Occured: {ex.InnerException}");
                    }
                }
        }

        public void ProcessPlayback(ConnectedDevice cd)
        {
            Console.WriteLine("**** Added Mac Address to playback list: " + cd.macaddress + " (Name: " + cd.hostname + ")");
            playbackHelper.addMacAddres(cd.macaddress);          
        }
        
        public void CheckForTimedOutConnections()
        {
            Console.WriteLine($"Looping through nonTimedOutDevices.  Looking for devices that have not been seen on the network in the last [{NetworkHelper.connectionTimeOutMinutes}] minutes");

            lock (masterDeviceList) lock (ConnectedDeviceList)
                {

                    foreach (ConnectedDevice cd in ConnectedDeviceList.Reverse<ConnectedDevice>())
                    {
                        if (cd.isTimedOut(NetworkHelper.connectionTimeOutMinutes))
                        {
                            ConnectedDeviceList.Remove(cd);
                            Console.WriteLine("DEVICE TIMED OUT - REMOVING: " + cd.macaddress + " from connected(non timed out) list");
                        }
                    }

                    System.IO.FileStream nonTimedOoutDevicesFile = System.IO.File.Open(ConnectedDeviceListXmlPath, FileMode.Truncate);
                    connectedDeviceSerializer.Serialize(nonTimedOoutDevicesFile, ConnectedDeviceList);
                    nonTimedOoutDevicesFile.Close();
                    System.IO.FileStream masterDeviceListFile = System.IO.File.Open(MasterDeviceListXmlPath, FileMode.Truncate);
                    connectedDeviceSerializer.Serialize(masterDeviceListFile, masterDeviceList);
                    masterDeviceListFile.Close();
                }
        }

        private static string NetworkGateway()
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

        private static string GetHostName(string ipAddress)
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

        private static string GetMacAddress(string ipAddress)
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