using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetworkScanner.Models;
using System.Threading.Tasks;
using System.Threading;

namespace NetworkScanner.Application
{
    public class NetworkOperations
    {
        public PlaybackApplication PlaybackAppliaction;
        public NetworkRepository NetworkRepository;
        public List<ConnectedDevice> MasterDeviceListBuffer;
        List<ConnectedDevice> ConnectedDeviceListBuffer;
        public List<ThemeSong> ThemeSongsBuffer;        
        public int ConnectedDeviceTimeoutTime;
        public int PingTimeOutMiliseconds;        
        public int PingRound;        

        public NetworkOperations(int pingTimeoutMiliseconds, int connectedDeviceTimeoutTime)
        {
            PlaybackAppliaction = new PlaybackApplication();
            NetworkRepository = new NetworkRepository();
            PingTimeOutMiliseconds = pingTimeoutMiliseconds;            
            ConnectedDeviceTimeoutTime = connectedDeviceTimeoutTime;
            MasterDeviceListBuffer = NetworkRepository.GetMasterDeviceList();
            ConnectedDeviceListBuffer = NetworkRepository.GetConnectedDeviceList();
            ThemeSongsBuffer = NetworkRepository.GetThemeSongsList();            
            PingRound = 0;            
        }

        public void Ping_all(bool updateThemeSongsBuffer)
        {
            PingRound++;
            
            if (updateThemeSongsBuffer)
            {
                ThemeSongsBuffer = NetworkRepository.GetThemeSongsList();
            }            

            Console.WriteLine($"-- Pinging All Possible Local Addresses.  Round: {PingRound}");

            string gate_ip = NetworkOperations.NetworkGateway();
            string[] array = gate_ip.Split('.');
            Parallel.For(2, 255, i =>
            {
                string host = array[0] + "." + array[1] + "." + array[2] + "." + i;
                Ping ping = new Ping();
                ping.PingCompleted += new PingCompletedEventHandler(PingHandler);
                ping.SendAsync(host, PingTimeOutMiliseconds);                
            });            
        }

        public void PingHandler(object sender, PingCompletedEventArgs e)
        {             
            PingReply pingResult = e.Reply;
            if (pingResult == null || pingResult.Status != IPStatus.Success)
            {
                return;
            }            

            string ip = pingResult.Address.ToString();                

            ConnectedDevice respondingConnectedDevice = new ConnectedDevice
            {                    
                ip = ip,
                hostname = GetHostName(ip),
                macaddress = GetMacAddress(ip),
                connectDateTime = DateTime.UtcNow,
            };

            bool processingComplete = false;
            int attempts = 1;
            while (!processingComplete)
            {                
                try
                {
                    if (MasterDeviceListBuffer.Any(x => x.macaddress == respondingConnectedDevice.macaddress))
                    {
                        //This macAddress has connected before. Lets make sure its ip address and username are up to date in the ip/mac lookup table. This data can be used by a web client to identify the mac address of a user by their ip address.                         
                        ConnectedDevice knownDevice = MasterDeviceListBuffer.Where(x => x.macaddress == respondingConnectedDevice.macaddress).FirstOrDefault();
                        string userName = ThemeSongsBuffer.Where(x => x.macAddress == knownDevice.macaddress).FirstOrDefault()?.userName ?? "unknown";
                        if(knownDevice.ip != respondingConnectedDevice.ip || knownDevice.userName != userName)
                        {                                
                            lock (MasterDeviceListBuffer)
                            {
                                knownDevice.ip = respondingConnectedDevice.ip;
                                knownDevice.userName = userName;
                                knownDevice.connectDateTime = DateTime.UtcNow;
                                MasterDeviceListBuffer = NetworkRepository.UpdateMasterDeviceListDevice(knownDevice);
                            }                                
                        }
                    }
                    else
                    {
                            
                        Console.WriteLine($"- New device detected. Adding to master device list - Host Name: {respondingConnectedDevice.hostname} - Mac Address: {respondingConnectedDevice.macaddress}");
                        respondingConnectedDevice.userName = ThemeSongsBuffer.Where(x => x.macAddress == respondingConnectedDevice.macaddress).FirstOrDefault()?.userName ?? "unknown";
                            
                        lock (MasterDeviceListBuffer)
                        {
                            MasterDeviceListBuffer = NetworkRepository.AddToMasterDeviceList(respondingConnectedDevice);                            
                        }                                                        
                    }
                    
                    if (ConnectedDeviceListBuffer.Any(x => x.macaddress == respondingConnectedDevice.macaddress))
                    {
                        // A device with an associated themesong has responded to a ping. In order for a themesong to be played, the device must go {connectedDeviceTimeoutTime} without responding to a ping.  Therefore, we will update its connectDateTime timestamp.                        
                        lock (ConnectedDeviceListBuffer)
                        {
                            Console.WriteLine($"- A device which has already played its themesong in the last timeout period has reconnected. Updating ConnectDateTime.   [Host Name: {respondingConnectedDevice.hostname}]");
                            ConnectedDeviceListBuffer = NetworkRepository.UpdateConnectedDeviceTimeStamp(respondingConnectedDevice);
                        }                        
                    }
                    else if (ThemeSongsBuffer.Any(x => x.macAddress == respondingConnectedDevice.macaddress))
                    {
                        //A device with an associated themesong has just connected. It has been over {connectedDeviceTimeoutTime} miliseconds since its last connection, so time to play their themesong!
                        string userName = ThemeSongsBuffer.Where(x => x.macAddress == respondingConnectedDevice.macaddress).FirstOrDefault()?.userName ?? "unknown";
                        respondingConnectedDevice.userName = userName;
                        lock (ConnectedDeviceListBuffer)
                        {
                            ConnectedDeviceListBuffer = NetworkRepository.AddToConnectedDeviceList(respondingConnectedDevice);
                            Console.WriteLine("**** Added Mac Address to playback list: " + respondingConnectedDevice.macaddress + " (UserName: " + respondingConnectedDevice.userName + ")");
                            PlaybackAppliaction.AddMacAddress(respondingConnectedDevice.macaddress);
                        }                             
                    }
                    processingComplete = true;                  
                    if (attempts > 1)
                    {
                        Console.WriteLine($"Took {attempts.ToString()} attempts to process ping response from {respondingConnectedDevice.ip}");
                    }
                }                
                catch
                {
                    attempts++;
                    Thread.Sleep(0);
                    //Lower level catches will log the error and throw.  Ping handler execution will halt, get caught here, then continue on to next ping. 
                }
            }
        }

        public void CheckForTimedOutConnections()
        {
            lock (ConnectedDeviceListBuffer)
            {
                List<ConnectedDevice> connectedDeviceList = NetworkRepository.GetConnectedDeviceList();

                foreach (ConnectedDevice cd in connectedDeviceList.Reverse<ConnectedDevice>())
                {
                    if (cd.IsTimedOut(ConnectedDeviceTimeoutTime))
                    {
                        connectedDeviceList.Remove(cd);
                        Console.WriteLine($"DEVICE TIMED OUT - REMOVING: {cd.userName} from connected device list list");
                    }
                }

                ConnectedDeviceListBuffer = connectedDeviceList;
                NetworkRepository.UpdateConnectedDeviceList(connectedDeviceList);
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