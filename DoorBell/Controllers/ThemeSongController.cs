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
using System.Web;
using System.Web.Hosting;


namespace DoorBell.Controllers
{
    public class ThemeSongsController : ApiController
    {
        private ThemeSong[] themeSongsTemp = new ThemeSong[]
        {
            new ThemeSong {macAddress = "device1", songYoutubeUrl = "device2"},
            new ThemeSong {macAddress = "device2", songYoutubeUrl = "device2"},
            new ThemeSong {macAddress = "device3", songYoutubeUrl = "device2"},
        };        

      //  public IEnumerable<ThemeSong> GetAllThemeSongs()
//        {
            //return themeSongs;
        //}
        
        [HttpPost]
        public IHttpActionResult postThemeSong(ThemeSong themeSong)
        {
            try
            {
                List<ThemeSong> themeSongs = new List<ThemeSong>() { };
                XmlSerializer themeSongSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<ThemeSong>));
                var themeSongsXmlPath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/Data/ThemeSongs.xml");
                using (XmlReader reader = XmlReader.Create(themeSongsXmlPath))
                {
                    themeSongs = (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                }

                themeSongs.Add(themeSong);

                System.IO.FileStream themeSongsXml = System.IO.File.Open(themeSongsXmlPath, FileMode.Truncate);
                themeSongSerializer.Serialize(themeSongsXml, themeSongs);
                themeSongsXml.Close();

                return Ok(themeSong);
            }
            catch
            {
                return InternalServerError();
            }            
        }
        
        
        [HttpGet]
        public IHttpActionResult getSongDuration(string videoUrl)
        {
            string duration = "";

            string tempUrl = "https://www.youtube.com/watch?v=bzjQvsn921k";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tempUrl);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                duration = reader.ReadToEnd();
            }
            int milisconds;
            //miliseconds = duration.toMiliseconds
            milisconds = 10000; //BUG not fully implemented 
            return Ok(milisconds);
        }
    }
}
