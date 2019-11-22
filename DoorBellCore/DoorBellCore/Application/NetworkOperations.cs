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
        public int ConnectedDeviceTimeoutTime;
        public int PingTimeOutMiliseconds;        
        public int PingRound;
        public bool threadlock;

        public NetworkOperations(int pingTimeoutMiliseconds, int connectedDeviceTimeoutTime)
        {
            PlaybackAppliaction = new PlaybackApplication();
            NetworkRepository = new NetworkRepository();
            PingTimeOutMiliseconds = pingTimeoutMiliseconds;            
            ConnectedDeviceTimeoutTime = connectedDeviceTimeoutTime;
            PingRound = 0;
            threadlock = false;
        }

        public void Ping_all()
        {
            PingRound++;
            Console.WriteLine($"-- Pinging All Possible Local Addresses.      Round: {PingRound}");

            string gate_ip = NetworkOperations.NetworkGateway();
            string[] array = gate_ip.Split('.');
            Parallel.For(2, 255, i =>
            {
                string host = array[0] + "." + array[1] + "." + array[2] + "." + i;
                Ping ping = new Ping();
                ping.PingCompleted += new PingCompletedEventHandler(PingHandler);
                ping.SendAsync(host, PingTimeOutMiliseconds);                
            });

            Console.WriteLine($"-- Done Pinging All Possible Local Addresses. Round: {PingRound}");
        }

        public void PingHandler(object sender, PingCompletedEventArgs e)
        {            
 
            PingReply pingResult = e.Reply;
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

            bool processingComplete = false;
            int attempts = 1;
            while (!processingComplete)
            {                
                try
                {
                    if (!threadlock)
                    {
                        threadlock = true;
                        List<ConnectedDevice> masterDeviceList = NetworkRepository.GetMasterDeviceList();
                        List<ThemeSong> themeSongs = NetworkRepository.GetThemeSongsList();
                        if (masterDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                        {
                            //This macAddress has connected before. Lets make sure its ip address and username are up to date in the ip/mac lookup table. This data can be used by a web client to identify the mac address of a user by their ip address. 
                            ConnectedDevice knownDevice = masterDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                            string userName = themeSongs.Where(x => x.macAddress == knownDevice.macaddress).FirstOrDefault()?.userName ?? "unknown";
                            knownDevice.ip = pingResults.ip;
                            knownDevice.userName = userName;
                        }
                        else
                        {
                            Console.WriteLine($"- New device detected. Adding to master device list - Host Name: {pingResults.hostname} - Mac Address: {pingResults.macaddress}");
                            pingResults.userName = themeSongs.Where(x => x.macAddress == pingResults.macaddress).FirstOrDefault()?.userName ?? "unknown";
                            masterDeviceList.Add(pingResults);
                            NetworkRepository.UpdateMasterDeviceList(masterDeviceList);
                        }

                        List<ConnectedDevice> connectedDeviceList = NetworkRepository.GetConnectedDeviceList();
                        if (connectedDeviceList.Any(x => x.macaddress == pingResults.macaddress))
                        {
                            //A device with an associated themesong has responded to a ping. In order for a themesong to be played, the device must go {connectedDeviceTimeoutTime} without responding to a ping.  Therefore, we will update its connectDateTime timestamp.
                            Console.WriteLine($"- A recognized connected device has reconnected. - Host Name: {pingResults.hostname} - Mac Address: {pingResults.macaddress}");
                            ConnectedDevice reconnectedDevice = connectedDeviceList.Where(x => x.macaddress == pingResults.macaddress).FirstOrDefault();
                            reconnectedDevice.connectDateTime = DateTime.UtcNow;
                        }
                        else if (themeSongs.Any(x => x.macAddress == pingResults.macaddress))
                        {
                            //A device with an associated themesong has just connected. It has been over {connectedDeviceTimeoutTime} miliseconds since its last connection, so time to play their themesong!
                            string userName = themeSongs.Where(x => x.macAddress == pingResults.macaddress).FirstOrDefault()?.userName ?? "unknown";
                            pingResults.userName = userName;
                            connectedDeviceList.Add(pingResults);
                            NetworkRepository.UpdateConnectedDeviceList(connectedDeviceList);
                            Console.WriteLine("**** Added Mac Address to playback list: " + pingResults.macaddress + " (UserName: " + pingResults.userName + ")");
                            PlaybackAppliaction.AddMacAddress(pingResults.macaddress);
                        }
                        processingComplete = true;
                        threadlock = false;
                        if (attempts > 1)
                        {
                            Console.WriteLine($"Took {attempts.ToString()} attempts to process ping response from {pingResults.ip}");
                        }
                    }
                    else
                    {
                        attempts++;
                        Thread.Sleep(0);
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
            List<ConnectedDevice> connectedDeviceList = NetworkRepository.GetConnectedDeviceList();            

            foreach (ConnectedDevice cd in connectedDeviceList.Reverse<ConnectedDevice>())
            {                
                if (cd.IsTimedOut(ConnectedDeviceTimeoutTime))
                {
                    connectedDeviceList.Remove(cd);
                    Console.WriteLine($"DEVICE TIMED OUT - REMOVING: {cd.userName} from connected device list list");
                }
            }            
            
            NetworkRepository.UpdateConnectedDeviceList(connectedDeviceList);
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