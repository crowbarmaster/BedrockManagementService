# Minecraft Bedrock management service
## An application suite for running and controlling multiple bedrock dedicated servers

## Project information

BedrockService started as a fork from Ravetroll's BedrockService, using TopShelf for multiple servers and a hidden server instance. This worked okay, but still required manual updates, lacked a way to monitor server instances, and using a WPF Console to merely monitor and send server commands was not enough for my wishes. It still would require you to interact with the server files directly, and if your server is remote, this can become a pain. My main goals for this project are as follows:

* A multiple server service run in the background. [Implemented, needs testing]
* Add/remove/modify/monitor servers from the GUI remotely. [Implemented]
* Both Service and individual server persistence over failure (AKA WatchDog). [implemented; needs further testing]
* Zero interaction with actual Minecraft server files and configs; all handled with service configuration. [Mostly implemented]
* Automatic server updates and deployment, all while leaving the configuration and added packs/worlds intact. [implemented]
* Automatic backup system, with the ability to manage backups and rollback via GUI. [implemented; needs further testing]
* Complete control of players, track known players, manage player permission and whitelist statuses and more. [Partially implemented, needs further testing]
* Pack manager to add/remove packs and worlds from servers via GUI. [Partially implemented, needs further testing]

TODO:
* Implement a way to modify level information. Not sure if I want to use an existing lib yet, or just add a couple static methods to change a few values (Ex. enabling experimental gameplay).
* Block more bad behavior in the GUI.
* Plans to add more information to the player manager. Implement sending commands to the running server to coincide with updated player settings (Ex. /op, /whitelist), currently servers require a restart before updated players can be recognized.
* Pack manager still needs a way to determine installed MCWorld packs, most likely adding a service-specific configuration file to come.
* Implement a way to ignore updates to enable old version persistence, as well as possibly allowing initial deployment of various versions.
* Client needs debug information written to file.
* Pull server props from new updates and apply. Currently using hard-coded copy and shouldn't be an issue as long as entries are not added to config in the future.
* Testing of all systems. There is still plenty of bugs to work out still, GUI hangs and plenty of bad behavior is possible. Please feel free to open an issue on GitHub with info of the issue from the logs.

## Quick-start guide

Download the latest release and extract to any directory. Run BedrockService.Service.exe, this will automatically download the latest copy of BDS (Bedrock Dedicated Server) from minecraft.net, extract to the default directory of C:\MCBedrockService, deploy stock settings and start the server. Once the new server is running, you can either close it and modify the configs with a text editor (see below for more details) or modify/add servers from the GUI Client.

## Service configuration files and directory layout

Default configurations are not shipped with the application. Please follow the quick-start guide to generate new config files. Once Generated, you will find that the paths Server and Service were created. 

* Server folder contains:
	* bedrock_ver.ini - keeps track of last downloaded version.
	* stock_packs.json - created after first use of pack manager, lists factory R/B packs on vanilla server.
	* Configs folder:
		* KnownPlayers folder:
			* This folder contains files with a name format of "ServerName.playerdb". See below for example file.
		* RegisteredPlayers folder:
			* This folder contains files with a name format of "ServerName.preg". See below for example file and usage.
		* Backups folder for configuration files.
		* Individual server configuration files.
	* MCSFiles folder:
		* stock_filelist.ini - contains a list of all files in vanilla build. Part of mod/world persist.
		* Update_x.xx.xx.xx.zip - Archive files downloaded from minecraft.net, x.xx.xx.xx is version of server.
	* Logs folder - Contains logs of only server outputs.
* Service folder contains:
	* Globals.conf - Global service configuration file.
	* Logs folder - Logs of service-specific outputs.
* Batch Folder - Contains a few batch files useful for installing/stating/stopping and removing the service from Windows Services. To use these please copy/move to BedrockService.Service.exe directory!
* BedrockService.Service.exe - core service executable.

All configuration files use a key=value layout. (Ex. ServersPath=C:\MCBedrockService). The parser will ignore any lines beginning with a "#" sign and empty lines for comments.

Globals.conf:

```
#Globals
ServersPath=C:\MCBedrockService // This is the path you wish to house your actual dedicated server files. Servers will deploy in a folder named after the chosen server’s name.
AcceptedMojangLic=false // Currently disabled! Will replace with disclaimer here in readme and remove this soon.
ClientPort=19134 // This is the port that will be used to listen for clients.
#Backups
BackupEnabled=false // Enable automatic backup system. Can still do manual one-click backups from GUI when disabled.
BackupPath=Default // Specify a path you wish to store server backups in. Leaving as Default will send your backups to "PathToService"\Servers\Backups\ServerName
BackupCron=0 1 * * * // Backup time interval. This is a Cron string, search "Cron time format" in your favorite search engine to find more details.
MaxBackupCount=25 // This number determines the maximum number of backups to maintain. Does not affect manual backups via GUI.
EntireBackups=false // Enabling this option will backup the entire server directory. Default "false" will backup only the "worlds" folder.
#Updates
CheckUpdates=true // Enabling this option turns on automatic updates.
UpdateCron=38 19 * * * // Update time interval. This is a Cron string, search "Cron time format" in your favorite search engine to find more details.
#Logging
LogServersToFile=false // Enabling this option will cause the service to write server logs to file.
LogServiceToFile=false // Enabling this option will cause the service to write the service log to file.
```

Server config file (Filename layout - if your server's name is “DefaultServer”, your config should be “DefaultServer.conf”)

```
# Server  
server-name=Default // These 24 entries are pulled directly from “server.properties”. Modify values to your liking, but do not modify anything left of the equals sign!
gamemode=creative
difficulty=easy
allow-cheats=false
max-players=10
online-mode=true
white-list=false
server-port=19132
server-portv6=19133
view-distance=32
tick-distance=4
player-idle-timeout=30
max-threads=8
level-name=Bedrock level
level-seed=
default-player-permission-level=member
texturepack-required=false
content-log-file-enabled=false
compression-threshold=1
server-authoritative-movement=server-auth
player-movement-score-threshold=20
player-movement-distance-threshold=0.3
player-movement-duration-threshold-in-ms=500
correct-player-movement=false

# StartCmds
Distracted=help 1 // Any of these optional fields will send following command to server on startup.
```

Registered player databases:
```
# Registered player list
# Register player entries: xuid,username,permission,isWhitelisted,ignoreMaxPlayers
# Example: 1234111222333444,TestUser,visitor,false,false

1234111222333444,TestUser,visitor,false,false
```
These files contain any players that are anything but default players. A default player is non-whitelisted with a player level equal to that set in the server’s configuration file. The comments included pretty much says what goes on here. You can add entries here manually after first start, or create the file and start from scratch. Players can also be added or modified in the GUI with the Player Manager.

Known player databases: 
```
1234111222333444,TestUser,637673952325785737,637673952325785737,637673952411885867
```

The service keeps track of player connection events and stores that information in these files. Currently only keeping track of connect and disconnect times. Current Layout: XUID,UserName,FirstConnectedTime,LastConnectedTime,DisconnectedTime. Times are in ticks.
This file is generated automatically, and while you can add entries to it, there is much reason to edit this file.

## Client configuration and directory layout

As with the service, The Client will generate a config on first run. Simply start and close the application, no connection is necessary.

* Client\Configs folder - Contains one file, Config.conf. Only used to configure hosts to connect to.
* BedrockService.Client.exe - core client executable.

## Configuring hosts
Client Config.conf:

```
[Hosts]
host1=127.0.0.1:19134
```

This file has one section defined by [Hosts]. Entry format: HostName=IPAddress:Port. You can change the name and address to whatever/wherever you wish. You Can also add more than one host for easy multiple host management.

The GUI Still has plenty of forms work to do, it’s a bit lower on the list. Constant progress will continue. With most everything I had initially hoped for currently working, I will be spending plenty of time weeding out bugs. Please feel free to drop me a message or post if you have any concerns or suggestions to offer on the MinecraftForum page located here:
https://www.minecraftforum.net/forums/minecraft/discussion/3120644-windows-bedrock-multi-host-multi-server-dedicated

