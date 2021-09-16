# BedrockService

# 2.0Beta notes!
## Readme is out of date! Current beta is not complete enough to post a readme yet. Hovever, some intuitive browsing through the default files that generate after the first run will give an easy idea of how things work. Progress has picked back up on this project. Expect a true release and a proper readme soon!

Moddified by Crowbarmaster brings you:

Windows Service Wrapper around Bedrock Server for Minecraft

Built on original code written by RaveTroll and supporters

This service wrapper for Minecraft Bedrock has been updated to provide

automatically applied updates, new configuration loader, and a process watchdog.

Initial installation:

Extract files to a directory of your chosing. If you want a quick-start jump-right-in experience,

launch BedrockService.exe, accept the terms, and watch your new server launch in seconds!

See below for a full, flexable, single or multi-server setup.

Configuration:

Server configuration(s) are found in the Configs folder. Default package ships with a "Globals.conf"

and "Default.conf". Configs are in plain-text format. Configuration sections are defined by wrapping them with "[]".

Globals File:

```
    [Globals]
    BackupEnabled=false // Enable backups here
    BackupCron=0 1 * * * // Set Cron interval for backups here
    AcceptedMojangLic=false // Mojang Licence accepted? NOTE: Must be set "true" here before installing as a service!
    CheckUpdates=true // Enable update routines here...
    UpdateCron=38 19 * * * // Set Cron interval for updates here
```
The Globals file contains just a few variables that will be shared between multiple servers.

Note the AcceptedMojangLic entry. In order to fetch downloads from minecraft.net, you must agree to the Terms

found at: https://minecraft.net/terms. If you plan to use this wrapper only to run multiple servers and NOT as a service,

ignore this. You will be asked in the console to accept the terms. Otherwise if you will run servers-as-a-service, this must

be set to "true" before running, or the build will not download.

Default Server File:

```
    [Service_Default]
    BedrockServerExeLocation=D:\MCSRV\MC1\
    BedrockServerExeName=bedrock_server.exe
    BackupFolderName=Default
    WCFPortNumber=19134

    [Server_Default]
    server-name=Test
    gamemode=creative
    difficulty=easy
    allow-cheats=false
    max-players=10
    online-mode=true
    white-list=true
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

    [Perms_Default]
    visitor=5555555555555555

    [Whitelist_Default]
    test=5555555555555555,false

    [StartCmds_Default]
    CmdDesc=help 1
```

This default file contains all information to run a single server, including minecraft-specific configs.

If you plan only to run a single server instance, you don’t need to change anything but what you need to.

To run more than a single server instance, create a copy of the default configuration file. The name of the

file you create does not matter, so long as the extension remains ".conf". In order to successfully launch

multiple servers:

    -All Config section definitions must be changed from "xxxxx_Default" to "xxxxx_YouDefineMe". "YouDefineMe" can be
    
    any char sequence/phase you wish and should be unique between servers.
    
    This will now be used by the wrapper to internally define this server.

    -Every entry found in the [Service_YouDefineMe] section must be both unique between servers, and point to unused resources.

    -The standard interference Minecraft settings, including server-name and ports, must be unique.

Everything else can be customized to your liking.

Perms_YouDefineMe & Whitelist_YouDefineMe are sections of the config that will push these files to the server root directory.

The Perms section will create entries in permissions.json. Each permission is formatted as DesiredPermissionLevel=PlayersXUID.

Default.conf's example:

```
[Perms_Default]
visitor=5555555555555555
```

Will output "permissions.json" to selected BedrockServerExeLocation that looks like:

```
[
	{
		"permission": "visitor",
		"xuid": "5555555555555555"
	}
]
```

All Whitelist entries will output to "whitelist.json" in the same formatting as Perms, however formatted in the config

a bit differently: MCUsername=XUID,IgnoresPlayerLimit

Example:

```
[Whitelist_Default]
test=5555555555555555,false
```

Translates to:

```
[
	{
		"username": "test",
		"xuid": "5555555555555555",
		"ignoresPlayerLimit": false
	}
]
```


Backups by default ("BackupFolderName=Default" in config) will backup servers to "backups\ServerShortName\Backup_DateString" unless changed to a custom path.

Backup system backs up the entire minecraft folder, in the case that resource packs are install, they are also backed up.


StartCmds are passable commandline startup options to this server. Each command defined in this format: CommandDescription=CommandToPass

CommandDescription is meaningless, and can be the same for each, or unique to you liking.

Service installation is simmilar to before, however you need not set permissions on the MC server folder, since it will be created by the service.

1.  You need to give permissions to SYSTEM to Modify the directory where BedrockService is located.

2.  Start a command prompt console with admin priviledges and navigate to the directory where you unzipped BedrockService.  
```
    Type: bedrockservice install   
    then
    Type: bedrockservice start
```    
If you need to uninstall BedrockService Start a command prompt console with admin priviledges and navigate to the directory where you unzipped BedrockService.
```
    Type: bedrockservice stop
    then
    Type: bedrockservice uninstall
```    

!! Notice: To ensure proper opperation, I highly reccomend you check "Run program as an administrator" in the Properties>Compatabity tab of BedrockService.exe !!
**************************************************************************************

Original Readme:

Windows Service Wrapper around Bedrock Server for Minecraft

Lets you run Bedrock Server as a Windows Service

This approach does NOT require Docker.

There is a Windows Server Software for Windows to allow users to run a multiplayer Minecraft server.

You can get it at https://www.minecraft.net/en-us/download/server/bedrock/

Its easy to install and runs as a console application.

What if you want it to run invisibly on your computer whenever it starts and shutdown statefully whenever your computer shuts?

Enter BedrockService, the little control program that performs just that task for you.  Download it here: https://github.com/ravetroll/BedrockService/raw/master/Releases/BedrockService.exe.zip

To configure it you have to do 4 things:

1.  Unzip the BedrockService.exe zip to a directory on your computer.

2.  You have to put the path to your copy of bedrock_server.exe in the BedrockService.exe.config file.  Make sure you have run your bedrock server in console mode first to be sure it works.

3.  You need to give permissions to SYSTEM to Modify both the directory with BedrockService as well as the directory containing bedrock_server.exe

4.  Start a command prompt console with admin priviledges and navigate to the directory where you unzipped BedrockService.  
```
    Type: bedrockservice install   
    then
    Type: bedrockservice start
```    
If you need to uninstall BedrockService Start a command prompt console with admin priviledges and navigate to the directory where you unzipped BedrockService.
```
    Type: bedrockservice stop
    then
    Type: bedrockservice uninstall
```    

If you have some problems getting the service running Check in Windows Event Log in the Application Events for events related to BedrockService.  That might help you find the problem.

See the wiki at https://github.com/ravetroll/BedrockService/wiki

