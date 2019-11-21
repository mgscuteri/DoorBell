using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using NetworkScanner.Models;
using System.Threading.Tasks;

namespace NetworkScanner.Application
{
    public class NetworkRepository
    {        
        public string ConnectedDeviceListXmlPath;
        public string MasterDeviceListXmlPath;
        public string ThemeSongsXmlPath;                

        public NetworkRepository()
        {   
            ConnectedDeviceListXmlPath = Directory.GetCurrentDirectory() + @"\data\ConnectedDevices.xml";
            MasterDeviceListXmlPath = Directory.GetCurrentDirectory() + @"\data\masterDeviceList.xml";
            ThemeSongsXmlPath = Directory.GetCurrentDirectory() + @"\Data\ThemeSongs.xml";            
        }

        public void UpdateConnectedDeviceList(List<ConnectedDevice> connectedDeviceList)
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));            
            try
            {
                FileStream connectedDeviceListXmlFile = File.Open(ConnectedDeviceListXmlPath, FileMode.Truncate);
                connectedDeviceSerializer.Serialize(connectedDeviceListXmlFile, connectedDeviceList);
                connectedDeviceListXmlFile.Close();                
            }
            catch 
            {
                Console.WriteLine($"Failed to update ConnectedDeviceList");
            }
            
        }

        public void UpdateMasterDeviceList(List<ConnectedDevice> masterDeviceList)
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));            
            try
            {
                FileStream masterDeviceListXmFile = File.Open(MasterDeviceListXmlPath, FileMode.Truncate);
                connectedDeviceSerializer.Serialize(masterDeviceListXmFile, masterDeviceList);
                masterDeviceListXmFile.Close();
            }
            catch 
            {
                Console.WriteLine($"Failed to update master device list");
            }

        }
        
        public List<ConnectedDevice> GetConnectedDeviceList()
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));            
            
            using (XmlReader reader = XmlReader.Create(ConnectedDeviceListXmlPath))
            {
                return (List<ConnectedDevice>)connectedDeviceSerializer.Deserialize(reader);
            }
        }

        public List<ConnectedDevice> GetMasterDeviceList()
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));            
            
            using (XmlReader reader = XmlReader.Create(MasterDeviceListXmlPath))
            {
                return (List<ConnectedDevice>)connectedDeviceSerializer.Deserialize(reader);
            }
        }

        public List<ThemeSong> GetThemeSongsList()
        {            
            XmlSerializer themeSongSerializer = new XmlSerializer(typeof(List<ThemeSong>));

            using (XmlReader reader = XmlReader.Create(ThemeSongsXmlPath))
            {
                return (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
            }            
        }        
    }
}