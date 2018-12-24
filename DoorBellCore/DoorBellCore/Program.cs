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
            Console.WriteLine("---- Starting DoorBell Server");
            //Initialize network helper
            NetworkHelper netHelper = new NetworkHelper();

            Stopwatch timer = new Stopwatch();
            timer.Start();

            int PingAllPollInterval = 3500;
            int CheckForTimedOutConnectionsInterval = 300000;


            while (true)
            {
                if(timer.ElapsedMilliseconds < CheckForTimedOutConnectionsInterval)
                {
                    Thread.Sleep(PingAllPollInterval);
                    netHelper.Ping_all();
                }
                else
                {
                    Console.WriteLine("------Checking for any timed out devices.");
                    Console.WriteLine("Waiting for previous pings to complete.");
                    Task.Delay(10000); //Wait 10 seconds for pings to complete
                    netHelper.CheckForTimedOutConnections();
                    timer.Restart();
                }
            }
        }
    }
}
