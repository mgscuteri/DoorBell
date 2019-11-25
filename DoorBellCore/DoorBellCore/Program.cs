using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NetworkScanner.Application;
using System.Threading;



namespace NetworkScanner
{
    class Program
    {
        public const int PingTimeoutMS = 1000;
        public const int PingAllPollIntervalMS = 1500;        
        public const int ConnectedDeviceTimeoutMS = 900000;
        public const int CheckForTimedOutConnectionsIntervalMS = 43200000;        

        static void Main(string[] args)
        {
            Console.WriteLine("Starting DoorBell Server");
            Start(PingTimeoutMS, PingAllPollIntervalMS, ConnectedDeviceTimeoutMS, CheckForTimedOutConnectionsIntervalMS);
        }

        private static void Start(int pingTimeoutMS, int pingAllPollInterval, int connectedDeviceTimeoutTime, int checkForTimedOutConnectionsInterval)
        {   
            NetworkOperations networkOperations = new NetworkOperations(pingTimeoutMS, connectedDeviceTimeoutTime);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (true)
            {
                networkOperations.Ping_all(false); // For the time being, restart the server to update theme song buffer buffer. (Need to write an api call for themesong management site that returns true if we need to update the buffer) 
                Thread.Sleep(pingAllPollInterval);

                if (timer.ElapsedMilliseconds > checkForTimedOutConnectionsInterval)
                {
                    Thread.Sleep(pingAllPollInterval * 2);
                    networkOperations.CheckForTimedOutConnections();
                    timer.Restart();
                }
            }
        }
    }
}
