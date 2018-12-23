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
using System.Web;


namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initialize network helper
            NetworkHelper netHelper = new NetworkHelper();

            while (true)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                Thread.Sleep(3500);
                netHelper.Ping_all();
                Console.WriteLine("~Ping All Took [" + timer.ElapsedMilliseconds.ToString() + "] miliseconds to complete");
            }
        }
    }
}
