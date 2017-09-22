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
        public string macaddress { get; set; }
        public DateTime connectDateTime { get; set; }
        public bool isNewConnection { get; set; }

        public ConnectedDevice()
        {

        }

        public bool isTimedOut(double timeOutMinutes)
        {
            var minutesElapsed = (DateTime.Now - connectDateTime).TotalMinutes;
                    
            return minutesElapsed > timeOutMinutes;
        }
    }
}