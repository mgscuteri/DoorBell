# DoorBell

This project consists of two main components.  The DoorBell web api, and the NetworkScanner.  

The Doorbell web api allows for api calls to be made to make additions to ThemeSongs.xml.
ThemeSongs.xml contains a list of macaddresses/and youtube urls to associate them with. 

The NetworkScanner polls the network with pings using the NetworkHelper class, which adds songs
to a playlist whenever a mac-address recongized in ThemeSongs.xml connects to the same router 
that the project is running on. It then launches a separate thread using the PlaybackHelper class,
to handle playing back the playlist. 
