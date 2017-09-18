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

        static void Main(string[] args)
        {

            while (1 == 1)
            {

                NetworkHelper netHelper = new NetworkHelper();

                netHelper.Ping_all();

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(NetworkHelper));

                string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "//data//connectedDevices.xml";

                System.IO.FileStream file = System.IO.File.Open(path,FileMode.Truncate);

                writer.Serialize(file, netHelper);
                file.Close();
                Thread.Sleep(pollingIntervalMiliseconds);
            }
        }
    }
}
