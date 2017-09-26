# DoorBell
__888****------THEMESONG SERVER------****888__

__---------.
Purpose   \_____
This project enables you to assign themesongs to yourself and your friends.  
The themesong will automatically play when the person to whom it belongs   
to enters wifi range.  

__---------------------.
Technical Overview   \_____
This project consists of two main components.  The DoorBell web api, and   
the NetworkScanner.    

The Doorbell web api allows for api calls to be made to make additions to  
ThemeSongs.xml. ThemeSongs.xml contains a list of macaddresses/and youtube  
urls to associate them with. 

The NetworkScanner polls the network with pings using the NetworkHelper  
class, which adds songs to a playlist whenever a mac-address recongized in  
ThemeSongs.xml connects to the same router that the project is running on.  
It then launches a separate thread using the PlaybackHelper class, to  
handle playing back the playlist.   

The NetworkScanner functions on a 2 hour timout. Meaning, after 2 hours  
without connecting to then network, the device will be dropped from the  
connected (nonTimedOut) device list, and be eligible to have its theme  
song played again.  Additionally, theme songs will not be played during a  
blackout period from 3:30am - 10:30am.
__---------------------.
Getting Started      \_____

To get started using the DoorBell server, first clone the repository,  
then:  
1) Right click on the solution, click startup properties, select  
multiple startup, then set both DoorBellApi and Network scanner to start  
2) Make an api call in this form to add a theme song to themeSongs.xml  (or modify manually)  
Type: Post  
Endpoint: http://localhost:64804/api/ThemeSongs/postThemeSong  
Headers: Content-Type: application/json  
Body: 
{
  "macAddress": 1,  
  "songYoutubeUrl" : "https://www.youtube.com/watch?v=8bCkadgRskA&list=RDGMEMYH9CUrFO7CfLJpaD7UR85wVM4L1sxibr8IA&index=4"  
}  
3) Connect to your wifi network.   
4) Listen to your themesong!  
