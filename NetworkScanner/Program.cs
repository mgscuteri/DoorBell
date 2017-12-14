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
        static void Main(string[] args)
        {
            //Initialize network helper
            NetworkHelper netHelper = new NetworkHelper();

            while (1 == 1)
            {
                try
                {
                    netHelper.Ping_all(NetworkHelper.pingTimeOutMiliseconds);                    
                }
                catch(Exception ex)
                {
                    //something went wrong
                }
             }
        }
    }
}
