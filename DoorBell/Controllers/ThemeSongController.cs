using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.DirectoryServices;
using DoorBell.Models;
using WebApplication10;

namespace DoorBell.Controllers
{
    public class ThemeSongsController : ApiController
    {
        private ThemeSong[] themeSongs = new ThemeSong[]
        {
            new ThemeSong {themeSongId = 1, deviceName = "device1", songUrl = "device2"},
            new ThemeSong {themeSongId = 1, deviceName = "device2", songUrl = "device2"},
            new ThemeSong {themeSongId = 1, deviceName = "device3", songUrl = "device2"},
        };

      //  public IEnumerable<ThemeSong> GetAllThemeSongs()
//        {
            //return themeSongs;
        //}

        public IHttpActionResult GetThemeSong(string deviceName)
        {

            ThemeSong themeSong = themeSongs.FirstOrDefault((p) => p.deviceName == deviceName);
            if (themeSong == null)
            {
                return NotFound();
            }

            NetworkHelper netHelper = new NetworkHelper();

            netHelper.Ping_all();



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

            return Ok(netHelper.SuccessfullPings);
        }
    }
}
