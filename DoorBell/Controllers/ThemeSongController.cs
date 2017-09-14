using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.DirectoryServices;
using DoorBell.Models;
using DoorBell;

namespace DoorBell.Controllers
{
    public class ThemeSongsController : ApiController
    {
        private ThemeSong[] themeSongs = new ThemeSong[]
        {
            new ThemeSong {macAddress = "device1", songYoutubeUrl = "device2"},
            new ThemeSong {macAddress = "device2", songYoutubeUrl = "device2"},
            new ThemeSong {macAddress = "device3", songYoutubeUrl = "device2"},
        };

      //  public IEnumerable<ThemeSong> GetAllThemeSongs()
//        {
            //return themeSongs;
        //}

        public IHttpActionResult GetThemeSong(string deviceName)
        {

            ThemeSong themeSong = themeSongs.FirstOrDefault((p) => p.macAddress == deviceName);
            if (themeSong == null)
            {
                return NotFound();
            }

           

      
            /*
            DirectoryEntry root = new DirectoryEntry("WinNT:");
            foreach (DirectoryEntry computers in root.Children)
            {
                foreach (DirectoryEntry computer in computers.Children)
                {
                    if (computer.Name != "Schema")
                    {
                        Console.WriteLine(computer.Name);
                    }
                }
            }
            */

            return Ok(themeSong);
        }
    }
}
