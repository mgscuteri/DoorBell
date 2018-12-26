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
            bool exitCondition = false;

            Console.WriteLine("Starting DoorBell Server");
            //Initialize network helper
            NetworkHelper netHelper = new NetworkHelper(pingAllPollInterval + 2000);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (!exitCondition)
            {
                if (timer.ElapsedMilliseconds < checkForTimedOutConnectionsInterval)
                {
                    netHelper.DeserializeDataStores();
                    Thread PingThread = new Thread(netHelper.Ping_all);
                    PingThread.Start();
                    Console.WriteLine($"Program Thread Sleeping");
                    Thread.Sleep(pingAllPollInterval);
                    Console.WriteLine($"Program Thread Waking");
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
