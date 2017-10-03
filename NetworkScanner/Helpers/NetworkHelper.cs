using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using NetworkScanner.Models;

namespace NetworkScanner.Helpers
{
    public class NetworkHelper
    {
        public List<ConnectedDevice> SuccessfullPings { get; set; }
        public int pingCounter { get; set; }


        public NetworkHelper()
        {
            SuccessfullPings = new List<ConnectedDevice> {};
        }
    

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
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
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

        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            pingCounter--;

            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname = GetHostName(ip);
                string macaddres = GetMacAddress(ip);
                string[] arr = new string[3];

                //store all three parameters to be shown on ListView
                arr[0] = ip;
                arr[1] = hostname;
                arr[2] = macaddres;

                // Logic for Ping Reply Success
                ConnectedDevice pingResults = new ConnectedDevice
                {
                    hostname = hostname,
                    ip = ip, macaddress = macaddres,
                    connectDateTime = DateTime.UtcNow,
                    isNewConnection = true
                };
                if(!SuccessfullPings.Any(x => x.macaddress == pingResults.macaddress))
                    SuccessfullPings.Add(pingResults);
            }
            else
            {
                // MessageBox.Show(e.Reply.Status.ToString());
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
                        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
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

        public void Ping_all(int pingTimeOutMiliseconds)
        {
            string gate_ip = NetworkHelper.NetworkGateway();
            lock(SuccessfullPings)
            {

            
            //Extracting and pinging all other ip's.
                string[] array = gate_ip.Split('.');

                for (int i = 2; i <= 255; i++)
                {

                    string ping_var = array[0] + "." + array[1] + "." + array[2] + "." + i;

                    //time in milliseconds           
                    Ping(ping_var, 1, pingTimeOutMiliseconds);
                }
            }
        }

    }
}