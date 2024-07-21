
This is an experiment to create a procedurally generated 
MUD/MMO fully streamed from the cloud using Unity/C#/Azure.

The code is MIT licensed and there is a tiny amount of cc0
licensed art.

You will need to download and install blender or the 3d models won't work.

Overview of projects:

AppConfig/App.Config -- Connection strings. 

FileUploader -- Utility to upload bundles and map data.
	
Genrpg.Editor -- Procedurally generated editor for player 
				and game data.
	
Genrpg.GameServer -- Main gameplay server that currently contains 
				several other servers for now. 
				(Instance/Monster/Player/MapServer). Look at
				GameServer.cs to see what they are.
			
Genrpg.InstanceServer -- Will eventually manage dynamically creating instances.
	
Genrpg.LoginServer -- Small website for logging, character management, and entering maps.

Genrpg.MapServer -- Server that can contain one or more MapInstances.
	Each MapInstance is a messaging system (a loose version of the Actor model)
			
To get an idea of how it works, look at
	
IMapMessageHandler, IServerMapService, IMapObjectManager
								
Genrpg.MonsterServer -- This will contain strategic AI/Faction code

Genrpg.PlayerServer -- This will handle player indexing and chat/who commands.

Genrpg.ServerShared -- Code used in all servers.

Genrpg.Shared -- Code shared between client and server. Copied to client during
				GameServer post-build step.

GenrpgClient -- Unity client with some of the core systems:

IScreenService handles UI
			
INetworkService handles data networking
			
IMapTerrainManager handles loading/unloading static map data
			
IClientObjectManager handles dynamic map objects (like units and spell effects)
			
IAssetService handles loading asset bundles and images and such
			
IUnityUpdateService - Provides a way to have frame updates without lots of
				Update/LateUpdate/UniTask/Coroutine functions.
								
IClientMapMessageHandler and BaseClientMapMessageHandler<T> 
			classes derived from/implementing these types handle messages sent from the
			map server.
			
GenrpgConfig -- Lambda that clients can use to determine their login server.

Setup	


	
1. AppConfig

	Contains App.config
	
	This contains the server config file with various connection strings.
	All nonclient projects reference this file to set up their connections on startup.
	
	This project will not run without these being hooked up someplace, but they could
	be swapped out for something on prem or on AWS or whatever you like
	
	As of now it contains the following:
		
	Blob Storage/S3:
		
	BlobContentConnection -- Stores data large data players can download like images, terrain chunks and asset bundles.
										(Should be public read/private write)
		
	(SQL HAS BEEN REMOVED IN LEIU OF USING ATOMIC INCREMENT IN NOSQL FOR ACCOUNT IDS)									
												
	Mongo/NoSQL:
		
	The current DB for this is serverless CosmosDB with a mongo frontend. As of this writing, I believe
		Mongo itself has a serverless MongoDBAtlas option, as well.

	NoSQLAccountDataConnection -- used for account data (moved from SQL)	
																		
	NoSQLWorldDataConnection -- used to store map gameplay data: generated maps, zones, spawns etc..
			
	NoSQLGameDataConnection -- used to store the core game metadata/settings like item list, monster data etc...
		
	NoSQLPlayerDataConnection -- used to store player data
		
	ServiceBusConnection -- This is used for internal cloud messaging (like SQS on AWS) for all things implementing ICloudMessage
	
	AzureRelayConnection -- This is used for bidirectional (request/response) cloud messaging, for ICloudRequest, ICloudResponse
						
	Classes may be tagged with a DataCategory attribute:
		
    [DataCategory(Category = DataCategory.AccountData)]   
     	
    [DataCategory(Category = DataCategory.PlayerData)]	    
    
	[DataCategory(Category = DataCategory.GameData)]	    
	
	[DataCategory(Category = DataCategory.Content)]
	    
	The way the type of database is determined (NoSQL vs Blob) is by the prefix on the connection string, so the
	    config string 
	    
	NoSQLGameDataConnection
	    
	makes all GameData datatypes get mapped to NoSQL using the connection string that follows.
	   	The "Connection" substring is removed, and then it looks for PlayerData, GameData, WorldData or Content,
	   	then removes that and the thing that's left is NoSQL or Blob and the connection string is used to
	   	connect to the proper storage.
	   	
2. Rebuild the FileUploader project. It will put the new executable int he proper folder to allow client uploads.	

3. Open the editor and click Genrpg-> Dev CopyToDB to set up the game DB

4. Open the GenrpgConfig project and look at Function1.cs It has some strngs for login server/asset root that
	the client initially connects to in order to get. You don't need to use this. Instead you could just short-circuit
	it at the top of InitClient.cs by creating a ConfigResponse object and passing it into the
	OnGetWebConfigAsync method. If you want to use the remote config system, you will need to change the
	ConfigURL in InitClient.cs from 
		
    const string ConfigURL = "https://yourremoteconfig.azurewebsites.net/api/GenrpgConfig";
    
    to whatever URL you want.
	
5. Start the LoginServer locally. For later on when you want to do a remote VM, you will need to change 
	TempDevConstants.TestVMIP to whatever server you are using (assuming the instance manager system
	hasn't been set up).

6. Open the GameMain project in GenrpgClient and look at the __Init object and the Config (Client Config) reference.
	Set the Env to local. 
	Set CurrMapId to 1
	Set Force Download From Asset Bundles to true
	Set Curr World Size to 4
	Set Curr Zone Size to 1
	Set Map Gen Seed to some random 32 bit integer.
	
	
7. If you set up the App.config BlobContentConnection and rebuilt the FileUploader, you should be able to 
	build asset bundles.
	
8. Click Build -> Dev Full Asset Build

9. Once that's done, enter play mode in Unity and Sign up making a new account.

10. Once you see the Character Select screen, create a Character (Do not click Gen yet)

11. Once you have a character, click Gen. This will take maybe 10-15 minutes for a map of size 4,
	and up to 12-15 hours for a map of size 80 (and require 30 gigs of ram). The ProcGen
	code is not very efficient)
	
12. Once the map is generated and uploaded (make sure you kept the LoginServer running), 
	restart the client and you should see a Map1 button next to your character
	If not, restart the LoginServer
	
13. You should have accountId=1, and should be able to edit your user in the editor.
	Click Genrpg->Dev Users and enter 1 in the Id entery field. It should find your
	user and let you click details to see it. Navigate through the button hierarch
	until you see your Character and set its level to 100. then click save.

	
14. Start the GameServer and wait a minute or so till it sets up its various subservers.

15. You should then be able to click Map1 next to your character and enter the world.
