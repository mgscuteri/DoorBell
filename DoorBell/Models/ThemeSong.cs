using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoorBell.Models
{
    public class ThemeSong
    {
        public int themeSongId { get; set; }
        public string deviceName { get; set; }
        public string songUrl { get; set; }
    }
}