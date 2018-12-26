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
            int checkForTimedOutConnectionsInterval = 900000; //15 minutes in miliseconds
            int connectedDeviceTimeoutTime = 43200000; //12 hours in miliseconds
            bool exitCondition = false;
            bool verboseLogging = false;

            Console.WriteLine("Starting DoorBell Server");
            //Initialize network helper
            NetworkHelper netHelper = new NetworkHelper(pingAllPollInterval + 2000, connectedDeviceTimeoutTime);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (!exitCondition)
            {
                if (timer.ElapsedMilliseconds < checkForTimedOutConnectionsInterval)
                {
                    netHelper.DeserializeDataStores();
                    Thread PingThread = new Thread(netHelper.Ping_all);
                    PingThread.Start();
                    if (verboseLogging) { Console.WriteLine($"Program Thread Sleeping"); }
                    Thread.Sleep(pingAllPollInterval);
                    if (verboseLogging) { Console.WriteLine($"Program Thread Waking"); }
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
