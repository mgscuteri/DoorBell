using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.DirectoryServices;
using DoorBell.Models;
using DoorBell;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


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
            // OLD \/
            //ThemeSong themeSong = themeSongs.FirstOrDefault((p) => p.macAddress == deviceName);
            //if (themeSong == null)
            //{
            //   return NotFound();
            //}

            //deserialize 
            XmlSerializer themeSongSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ThemeSong>));
            string themeSongsXmlPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "//data//ThemeSongs.xml";
            using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
            {
                themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
            }

            


            //System.IO.FileStream themeSongsXml = System.IO.File.Open(connectedDevicesXmlPath, FileMode.Truncate);
            //connectedDeviceListSerializer.Serialize(file, netHelper.SuccessfullPings);
            //file.Close();


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
