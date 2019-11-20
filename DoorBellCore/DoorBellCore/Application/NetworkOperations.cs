using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetworkScanner.Models;
using System.Threading.Tasks;

namespace NetworkScanner.Application
{
    public class NetworkOperations
    {
        public PlaybackApplication PlaybackAppliaction;
        public NetworkRepository NetworkRepository;
        public int ConnectedDeviceTimeoutTime;
        public int PingTimeOutMiliseconds;        
        public List<ConnectedDevice> ConnectedDeviceList ;
        public List<ConnectedDevice> MasterDeviceList ;
        public List<ThemeSong> ThemeSongs;        

        public NetworkOperations(int pingTimeoutMiliseconds, int connectedDeviceTimeoutTime)
        {
            PlaybackAppliaction = new PlaybackApplication();
            NetworkRepository = new NetworkRepository();
            ConnectedDeviceList = new List<ConnectedDevice> { };
            MasterDeviceList = new List<ConnectedDevice> { };
            ThemeSongs = new List<ThemeSong> { };
            PingTimeOutMiliseconds = pingTimeoutMiliseconds;            
            ConnectedDeviceTimeoutTime = connectedDeviceTimeoutTime;            
        }

        public void Ping_all()
        {
            Console.WriteLine("-- Pinging All Possible Local Addresses");
            Stopwatch readTimer = new Stopwatch();
            readTimer.Start();
            ConnectedDeviceList = NetworkRepository.GetConnectedDeviceList();
            MasterDeviceList = NetworkRepository.GetMasterDeviceList();
            ThemeSongs = NetworkRepository.GetThemeSongsList();
            Console.WriteLine($"Spent {readTimer.ElapsedMilliseconds} miliseconds reading previous ping data from disc");
            readTimer.Stop();


            Stopwatch pingTimer = new Stopwatch();
            pingTimer.Start();
            string gate_ip = NetworkOperations.NetworkGateway();
            string[] array = gate_ip.Split('.');
            Parallel.For(2, 255, i =>
            {
                string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;
                Ping(ping_var, 1, PingTimeOutMiliseconds);
            });
            Console.WriteLine($"Spent {pingTimer.ElapsedMilliseconds} miliseconds pinging all possible local ip addresses");
            pingTimer.Stop();


            Console.WriteLine("Writing ");
            Stopwatch writeTimer = new Stopwatch();
            writeTimer.Start();
            NetworkRepository.UpdateConnectedDeviceList(ConnectedDeviceList);
            NetworkRepository.UpdateMasterDeviceList(MasterDeviceList);
            Console.WriteLine($"Spent {writeTimer.ElapsedMilliseconds} miliseconds writing ping response data to disc");
            writeTimer.Stop();            

            Console.WriteLine("-- Done Pinging All Possible Local Addresses");
        }

        public void Ping(string host, int attempts, int timeout)
        {            
            try
            {
                Ping ping = new Ping();                
                PingReply pingResult = ping.Send(host, timeout);
                if (pingResult == null || pingResult.Status != IPStatus.Success)
                {
                    return;
                }                
                
                string ip = pingResult.Address.ToString();                

                ConnectedDevice pingResults = new ConnectedDevice
                {                    
                    ip = ip,
                    hostname = GetHostName(ip),
                    macaddress = GetMacAddress(ip),
                    connectDateTime = DateTime.UtcNow,
                };

                if (MasterDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                {
                    //This macAddress has connected before. Lets make sure its ip address and username are up to date in the ip/mac lookup table. This data can be used by a web client to identify the mac address of a user by their ip address. 
                    ConnectedDevice knownDevice = MasterDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                    string userName = ThemeSongs.Where(x => x.macAddress == knownDevice.macaddress).FirstOrDefault()?.userName ?? "unknown";
                    knownDevice.ip = pingResults.ip;
                    knownDevice.userName = userName;                    
                }
                else
                {
                    Console.WriteLine($"- - * * New device detected. Adding to master device list - Host Name: {pingResults.hostname} - Mac Address: {pingResults.macaddress}");
                    pingResults.userName = ThemeSongs.Where(x => x.macAddress == pingResults.macaddress).FirstOrDefault()?.userName ?? "unknown";
                    MasterDeviceList.Add(pingResults);
                }

                if (ConnectedDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                {
                    //A device with an associated themesong has responded to a ping. In order for a themesong to be played, the device must go {connectedDeviceTimeoutTime} without responding to a ping.  Therefore, we will update its connectDateTime timestamp.
                    ConnectedDevice reconnectedDevice = ConnectedDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                    reconnectedDevice.connectDateTime = DateTime.UtcNow;
                }
                else if (ThemeSongs.Any(x => x.macAddress == pingResults.macaddress))
                {
                    //A device with an associated themesong has just connected. It has been over {connectedDeviceTimeoutTime} miliseconds since its last connection, so time to play their themesong!
                    string userName = ThemeSongs.Where(x => x.macAddress == pingResults.macaddress).FirstOrDefault()?.userName ?? "unknown";
                    pingResults.userName = userName;
                    Console.WriteLine($"* * * * - -- - * * * * New device detected. Adding to master device list - UserName: {userName}  MacAddress: {pingResults.macaddress}");
                    ConnectedDeviceList.Add(pingResults);                    
                    Console.WriteLine("**** Added Mac Address to playback list: " + pingResults.macaddress + " (UserName: " + pingResults.userName + ")");
                    PlaybackAppliaction.AddMacAddress(pingResults.macaddress);
                }
            }
            catch
            {
                Console.WriteLine($"Encountered difficulty while processing the ping of host: {host} ");
            }            
        }

        public void CheckForTimedOutConnections()
        {
            ConnectedDeviceList = NetworkRepository.GetConnectedDeviceList();
            MasterDeviceList = NetworkRepository.GetMasterDeviceList();

            foreach (ConnectedDevice cd in ConnectedDeviceList.Reverse<ConnectedDevice>())
            {                
                if (cd.IsTimedOut(ConnectedDeviceTimeoutTime))
                {
                    ConnectedDeviceList.Remove(cd);
                    Console.WriteLine($"DEVICE TIMED OUT - REMOVING: {cd.userName} from connected device list list");
                }
            }            
            
            NetworkRepository.UpdateConnectedDeviceList(ConnectedDeviceList);
            NetworkRepository.UpdateMasterDeviceList(MasterDeviceList);
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