# DoorBell

This application, which I am simply calling the DoorBell for now, is designed to alert you when someone you know is nearby by playing a themesong of their choosing. If multiple people arrive simultaneously, it will queue each song to be played in the order the people were detected. It works by associating macaddresses with songs.  When a person's phone detects a known wifi network it automatically connects. This application, when running on the same network, will detect this connection by continuously polling the network.  Once a device connects, it is designated "connected", and will not be designated "disconnected" until 12 hours have passed without seeing the device on the network.  

Roadmap:  
☑ Create mechanism discover new network connections  
☑ Create mechanism to play songs  
☑ Add ability to play song when recognized mac address connects to network  
☑ Create mechanism to queue songs when multiple devices connect in rapid succsion  
☑ Add Connect/Disconnect Logic that prevents playback from occuring at unwanted times  
☑ Port to .net core  
☑ Scrutinize/Improve threading/asyncs for mimimal CPU utilization  
☐ Host on Raspberry Pi  
☐ Port user-song/macaddress associations from XML file to database  
☐ Integrate with https://github.com/mgscuteri/PythonPoweredPi (user-song/macaddress pairs will come from this website)  

