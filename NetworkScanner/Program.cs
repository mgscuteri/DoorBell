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

namespace NetworkScanner
{
    class Program
    {
        public const int pollingIntervalMiliseconds = 750;
        public const int pingTimeOutMiliseconds = 500;
        public const int connectionTimeOutMinutes = 120;


        static void Main(string[] args)
        {

            while (1 == 1)
            {

                NetworkHelper netHelper = new NetworkHelper();
                netHelper.pingCounter = 253;


                netHelper.Ping_all(pingTimeOutMiliseconds);
                //Launch all pings
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(NetworkHelper));
                string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "//data//connectedDevices.xml";

                foreach(ConnectedDevice cd in netHelper.SuccessfullPings)
                {
                    if (cd.isTimedOut(connectionTimeOutMinutes))
                        netHelper.SuccessfullPings.Remove(cd);
                }


                //Wait for pings to finish -- ANY WORK not dependent on ping responses should go ABOVE here!  
                while (netHelper.pingCounter > 0)
                {
                    //wait
                }

                System.IO.FileStream file = System.IO.File.Open(path,FileMode.Truncate);
                writer.Serialize(file, netHelper);
                file.Close();
                Thread.Sleep(pollingIntervalMiliseconds);
            }
        }
    }
}
