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
    }
}