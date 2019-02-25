# DoorBell  

**Summary**  
This application is designed to alert you when someone you know is nearby.  It does this by playing a song of their choosing. If multiple people arrive simultaneously, it will dynamically create a queue.  


**How it works**  
It works by associating macaddresses with songs.  When a person's phone detects a known wifi network it automatically connects. This application, when running on the same network, will detect this connection by continuously polling the network.  Once a device connects, it is designated "connected", and will not be designated "disconnected" until 12 hours have passed without seeing the device on the network.  

**Roadmap:**    
☑ Create mechanism discover new network connections  
☑ Create mechanism to play songs  
☑ Add ability to play song when recognized mac address connects to network  
☑ Create mechanism to queue songs when multiple devices connect in rapid succsion  
☑ Add Connect/Disconnect Logic that prevents playback from occuring at unwanted times  
☑ Port to .net core  
☑ Scrutinize/Improve threading/asyncs for mimimal CPU utilization  
☐ Host on Raspberry Pi (Port playback script to bash) 
☐ Add ability to determine a phone's mac address based on its ip address
☐ Port user-song/macaddress associations from XML file to database  
☐ Integrate with https://github.com/mgscuteri/PythonPoweredPi (user-song/macaddress pairs will come from this website)  

