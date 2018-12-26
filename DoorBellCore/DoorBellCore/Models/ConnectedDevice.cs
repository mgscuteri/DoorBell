using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetworkScanner.Models
{
    public class ConnectedDevice
    {
        public string ip { get; set; }
        public string hostname { get; set; }
        public string userName { get; set; }
        public string macaddress { get; set; }
        public DateTime connectDateTime { get; set; }

        public ConnectedDevice()
        {

        }

        public bool IsTimedOut(int timeOutMiliseconds)
        {
            var milisecondsElapsed = (int)Math.Floor((DateTime.UtcNow - connectDateTime).TotalMilliseconds);
            Console.WriteLine($"Checking {userName} for timeout. Elapsed Miliseconds: {milisecondsElapsed} ");
            bool isTimedOut = milisecondsElapsed > timeOutMiliseconds;
            Console.WriteLine($"IsTimedOut: {isTimedOut} ");
            return isTimedOut;
        }
    }
}