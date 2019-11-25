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
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to update ConnectedDeviceList");
                throw ex;
            }            
        }

        public List<ConnectedDevice> UpdateConnectedDeviceTimeStamp(ConnectedDevice reconnectedDevice)
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
            List<ConnectedDevice> connectedDevices = GetConnectedDeviceList();

            try
            {                
                connectedDevices.Where(x => x.macaddress == reconnectedDevice.macaddress).FirstOrDefault().connectDateTime = DateTime.UtcNow;
                FileStream connectedDeviceListXmlFile = File.Open(ConnectedDeviceListXmlPath, FileMode.Truncate);
                connectedDeviceSerializer.Serialize(connectedDeviceListXmlFile, connectedDevices);
                connectedDeviceListXmlFile.Close();
                return connectedDevices;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to update ConnectedDeviceList");
                throw ex;
            }
        }

        public List<ConnectedDevice> AddToConnectedDeviceList(ConnectedDevice newDevice)
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
            List<ConnectedDevice> connectedDevices = GetConnectedDeviceList();
            try
            {            
                FileStream connectedDeviceListXmFile = File.Open(ConnectedDeviceListXmlPath, FileMode.Truncate);
                connectedDevices.Add(newDevice);
                connectedDeviceSerializer.Serialize(connectedDeviceListXmFile, connectedDevices);
                connectedDeviceListXmFile.Close();
                return connectedDevices;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed Add to ConnectedDeviceList");
                throw ex;
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

        public List<ConnectedDevice> UpdateMasterDeviceListDevice(ConnectedDevice updatedDevice)
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
            List<ConnectedDevice> masterDeviceListDevices = GetMasterDeviceList();

            try
            {
                ConnectedDevice oldDevice = masterDeviceListDevices.Where(x => x.macaddress == updatedDevice.macaddress).FirstOrDefault();
                oldDevice = updatedDevice;
                FileStream masterDeviceListXmlFile = File.Open(MasterDeviceListXmlPath, FileMode.Truncate);
                connectedDeviceSerializer.Serialize(masterDeviceListXmlFile, masterDeviceListDevices);
                masterDeviceListXmlFile.Close();
                return masterDeviceListDevices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update ConnectedDeviceList");
                throw ex;
            }
        }


        public List<ConnectedDevice> AddToMasterDeviceList(ConnectedDevice newDevice)
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));
            List<ConnectedDevice> masterDeviceList = GetMasterDeviceList();
            if(masterDeviceList.Where(x => x.macaddress == newDevice.macaddress).Count() > 0)
            {
                Console.WriteLine("Detected attempt to add duplicate record to MasterDeviceList");
                return masterDeviceList;
            }
            try
            {   
                FileStream masterDeviceListXmFile = File.Open(MasterDeviceListXmlPath, FileMode.Truncate);
                masterDeviceList.Add(newDevice);                
                connectedDeviceSerializer.Serialize(masterDeviceListXmFile, masterDeviceList);
                masterDeviceListXmFile.Close();
                return masterDeviceList;
            }
            catch
            {
                Console.WriteLine($"Failed to add to MasterDeviceList");
                return masterDeviceList;
            }
        }


        public List<ConnectedDevice> GetConnectedDeviceList()
        {            
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));

            try
            {
                using (XmlReader reader = XmlReader.Create(ConnectedDeviceListXmlPath))
                {
                    return (List<ConnectedDevice>)connectedDeviceSerializer.Deserialize(reader);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed to read connected device list");
                throw ex;
            }
            
        }

        public List<ConnectedDevice> GetMasterDeviceList()
        {
            XmlSerializer connectedDeviceSerializer = new XmlSerializer(typeof(List<ConnectedDevice>));

            try
            {
                using (XmlReader reader = XmlReader.Create(MasterDeviceListXmlPath))
                {
                    return (List<ConnectedDevice>)connectedDeviceSerializer.Deserialize(reader);
                }
            }
            catch(Exception ex)
            {
                Console.Write("Failed to get master device list");
                throw ex;
            }            
        }

        public List<ThemeSong> GetThemeSongsList()
        {            
            XmlSerializer themeSongSerializer = new XmlSerializer(typeof(List<ThemeSong>));

            try
            {
                using (XmlReader reader = XmlReader.Create(ThemeSongsXmlPath))
                {
                    return (List<ThemeSong>)themeSongSerializer.Deserialize(reader);
                }
            }
            catch(Exception ex)
            {
                Console.Write("Failed to get themesong list");
                throw ex;
            }            
        }        
    }
}