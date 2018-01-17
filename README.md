# DoorBell
<<<< THEMESONG SERVER README >>>>

__~Purpose__  
This project enables you to assign themesongs to yourself and your friends.  
The themesong will automatically play when the person to whom it belongs   
to enters wifi range.  

__~Technical Overview__  
This project consists of three main components. The NetworkScanner, 
the PlaybackHelper, and the DoorbellWebApi(still under development).    

The purpuse of the DoorbellWebApi is to popluate ThemeSongs.xml, a 
key value pair table that associates a device's mac-address (key) with
the URL of a song (typically a youtube url). 

The NetworkScanner pings every possible address that the router it
is connected to can assign about once every 4 seconds. When a recognized
mac-address responds to one of these pings, that mac-address is sent over
to the PlaybackHelper, which takes care of playing the song.

The NetworkScanner is also smart enough to differnetiate between new 
connections and reconnections.  Once a device connects to the network,
it will be considered connected for a minimum of 2 hours. If the device
has not reconnected in this time frame, the connection will time-out.
Additionally, theme songs will not be played during a blackout period 
from 3:30am - 10:30am.  

__~Getting Started__   
To get started using the DoorBell server, first clone the repository,  
then:  
1) Right click on the solution, click startup properties, select  
multiple startup, then set both DoorBellApi and Network scanner to start  
2) Make an api call in this form to add a theme song to themeSongs.xml  
(or add to it manually!)  
Type: Post  
Endpoint: http://localhost:64804/api/ThemeSongs/postThemeSong  
Headers: Content-Type: application/json  
Body: 
{
  "macAddress": 1a-1a-1a-1a-1a-1a,  
  "songYoutubeUrl" : "https://www.youtube.com/watch?v=8bCkadgRskA&list=RDGMEMYH9CUrFO7CfLJpaD7UR85wVM4L1sxibr8IA&index=4"  
}  
3) Connect to your wifi network.   
4) Listen to your themesong!  
