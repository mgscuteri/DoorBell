using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NetworkScanner.Helpers;
using System.Threading;



namespace NetworkScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
        }

        private static void Start()
        {
            int pingAllPollInterval = 3500;
            int checkForTimedOutConnectionsInterval = 300000;

            Console.WriteLine("Starting DoorBell Server");
            //Initialize network helper
            NetworkHelper netHelper = new NetworkHelper(pingAllPollInterval + 2000);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (true)
            {
                if (timer.ElapsedMilliseconds < checkForTimedOutConnectionsInterval)
                {
                    netHelper.DeserializeDataStores();
                    Thread PingThread = new Thread(netHelper.Ping_all);
                    PingThread.Start();
                    Console.WriteLine($"Waiting for poll interval: {pingAllPollInterval}");
                    Thread.Sleep(pingAllPollInterval);
                    netHelper.SerializeDataStores();
                }
                else
                {
                    Console.WriteLine("Checking for any timed out devices.");
                    Console.WriteLine("Waiting for previous pings to complete.");
                    netHelper.CheckForTimedOutConnections();
                    timer.Restart();
                }
            }
        }
    }
}
