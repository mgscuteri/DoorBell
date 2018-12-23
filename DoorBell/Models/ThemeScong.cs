﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkScanner.Models
{
    public class ThemeSong
    {
        public string macAddress { get; set; }
        public string songYoutubeUrl { get; set; }
        public double minutesToPlay { get; set; }
        public string startMinutesSeconds { get; set; } //?t=11m10s
        public ThemeSong()
        {
            startMinutesSeconds = "";
        }
    }
}
