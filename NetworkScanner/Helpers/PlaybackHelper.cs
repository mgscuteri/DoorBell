using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoorBell.Models;

namespace NetworkScanner.Helpers
{
    

    class PlaybackHelper
    {
        public List<string> playListMacs {get; set;}        

        public PlaybackHelper()
        {
            playListMacs = new List<string> { };
        }
    }
    
}
