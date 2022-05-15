using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Utility;
using System.Net.Sockets;
using System.Numerics;

// Much of packet documentation is from Terraria's NetMessage.cs and from https://tshock.readme.io/docs/multiplayer-packet-structure
namespace HeadlessTerrariaClient.Client
{
    /// <summary>
    /// A Terraria client without any overhead
    /// </summary>
    public class HeadlessClient
    {
        /// <summary>
        /// The TCP client to connect to the server with
        /// </summary>
        public ArkTCPClient TCPClient { get; private set; }

        private bool DisconnectFromServer { get; set; }

        /// <summary>
        /// Current state in the connection protocol
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Buffer used for writing to the NetworkStream
        /// </summary>
        public byte[] WriteBuffer = new byte[131070];

        /// <summary>
        /// Buffer used for reading from the NetworkStream
        /// </summary>
        public byte[] ReadBuffer = new byte[131070];

        public MemoryStream MemoryStreamWrite { get; private set; }
        public BinaryWriter MessageWriter { get; private set; }

        public MemoryStream MemoryStreamRead { get; private set; }
        public BinaryReader MessageReader {get; private set; }

        /// <summary>
        /// Event called after the WorldData packet is received
        /// </summary>
        public Action<HeadlessClient> WorldDataRecieved { get; set; }

        /// <summary>
        /// Event called after the FinishedConnectingToServer packet is received
        /// </summary>
        public Action<HeadlessClient> FinishedConnectingToServer { get; set; }

        /// <summary>
        /// Event called after the CompleteConnectionAndSpawn packet is received
        /// </summary>
        public Action<HeadlessClient> ClientConnectionCompleted { get; set; }

        /// <summary>
        /// Event called every time the game loop runs
        /// </summary>
        public Action<HeadlessClient> OnUpdate { get; set; }

        /// <summary>
        /// Event called when a chat message is received
        /// </summary>
        public Action<HeadlessClient, ChatMessage> ChatMessageRecieved { get; set; }

        /// <summary>
        /// Event called when another player manipulates a tile.
        /// Returns a boolean of whether or not to process this tile event normally
        /// </summary>
        public Func<HeadlessClient, TileManipulation, bool> TileManipulationMessageRecieved { get; set; }

        /// <summary>
        /// Event called when any packet is received
        /// </summary>
        public Action<HeadlessClient, RawIncomingPacket> NetMessageReceived { get; set; }

        /// <summary>
        /// Event called when any packet is sent
        /// </summary>
        public Action<HeadlessClient, RawOutgoingPacket> NetMessageSent { get; set; }

        /// <summary>
        /// A reference to a ClientWorld
        /// </summary>
        public ClientWorld World { get; set; }

        /// <summary>
        /// The current index of this client's player
        /// </summary>
        private int myPlayer = 0;

        /// <summary>
        /// The current Player object for this client
        /// </summary>
        public Player LocalPlayer
        {
            get
            {
                return World.Players[myPlayer];
            }
        }

        /// <summary>
        /// The GUID for this client
        /// </summary>
        public string clientUUID { get; set; }

        /// <summary>
        /// this game doodoo
        /// </summary>
        public bool ServerSideCharacter { get; private set; }

        /// <summary>
        /// why is this here
        /// </summary>
        public ulong LobbyId { get; private set; }

        /// <summary>
        /// The version of the game to use
        /// </summary>
        public int VersionNumber
        {
            get
            {
                return 248;
            }
        }

        /// <summary>
        /// Returns whether or not the client is in a world
        /// </summary>
        public bool IsInWorld { get; private set; }

        /// <summary>
        /// Dynamic settings object
        /// </summary>
        public dynamic Settings { get; private set; }

        public HeadlessClient()
        {
            ConnectionState = ConnectionState.None;
            Settings = new Settings();
            SetDefaultSettings();
        }

        /// <summary>
        /// Sets the default settings
        /// </summary>
        public void SetDefaultSettings()
        {
            // Printing out anything to Console.WriteLine
            Settings.PrintAnyOutput = true;
            Settings.PrintPlayerId = false;
            Settings.PrintWorldJoinMessages = true;
            Settings.PrintUnknownPackets = false;
            Settings.PrintKickMessage = true;
            Settings.PrintDisconnectMessage = true;

            // Automatically send the SpawnPlayer packet
            Settings.SpawnPlayer = true;

            // Run a seperate game loop
            Settings.RunGameLoop = true;
            Settings.UpdateTimeout = 200;

            // Automatically send some information to the server that vanilla clients usually send, this can prevent some detection by anti-cheats
            Settings.AutoSyncPlayerZone = true;
            Settings.AutoSyncPlayerControl = false;
            Settings.AutoSyncPlayerLife = true;
            Settings.AutoSyncPeriod = 2000;
            Settings.LastSyncPeriod = DateTime.Now;

            // Load the actual tiles of the world, if this is set to false it will still have to decompress the tile section and will still keep track of what tile sectiosn you have loaded, but won't fill any tiles
            Settings.LoadTileSections = true;

            // Completely ignore all tile chunk packets
            Settings.IgnoreTileChunks = false;
        }

        /// <summary>
        /// Connects to a terraria server
        /// <param name="address">the address to connect to, both IP and domain names are supported</param>
        /// <param name="port">the port to connect on</param>
        /// </summary>
        public async Task Connect(string address, short port)
        {
            if (!ResolveIP(address, out IPAddress ipAddress))
            {
                throw new ArgumentException($"Could not resolve ip {address}");
            }

            TCPClient = new ArkTCPClient(ipAddress, ReadBuffer, port, OnRecieve);
            MemoryStreamWrite = new MemoryStream(WriteBuffer);
            MessageWriter = new BinaryWriter(MemoryStreamWrite);

            await ConnectToServer();
            BeginJoiningWorld();

            if (Settings.RunGameLoop)
            {
                Task.Run(
                    async () =>
                    {
                        while (true)
                        {
                            await Update();
                            await Task.Delay(Settings.UpdateTimeout);
                        }
                    });
            }

        }

        /// <summary>
        /// Connects the TCP client
        /// </summary>
        private async Task ConnectToServer()
        {
            await TCPClient.Connect();
            int retryCount = 0;
            while (!TCPClient.client.Connected || TCPClient.NetworkStream == null)
            {
                retryCount++;
                if (retryCount > 10)
                {
                    if (Settings.PrintAnyOutput && Settings.PrintConnectionMessages)
                    {
                        Console.WriteLine($"Error connecting to {TCPClient.IPAddress}:{TCPClient.port}");
                    }
                    return;
                }
                else
                {
                    await Task.Delay(100);
                }
            }

            MemoryStreamRead = new MemoryStream(ReadBuffer);
            MessageReader = new BinaryReader(MemoryStreamRead);
        }

        /// <summary>
        /// Starts joining a world
        /// </summary>
        private void BeginJoiningWorld()
        {
            SendData(1);
            ConnectionState = ConnectionState.SyncingPlayer;
        }


        /// <summary>
        /// Parses a string for the ip
        /// </summary>
        /// <param name="remoteAddress">IP address or domain name as a string</param>
        /// <param name="address">the IPAddress object resolved</param>
        /// <returns>whether the IP or domain name was valid</returns>
        public bool ResolveIP(string remoteAddress, out IPAddress address)
        {
            if (IPAddress.TryParse(remoteAddress, out address))
            {
                return true;
            }
            IPAddress[] addressList = Dns.GetHostEntry(remoteAddress).AddressList;
            for (int i = 0; i < addressList.Length; i++)
            {
                if (addressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    address = addressList[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handler for receiving bytes from the server
        /// </summary>
        /// <param name="bytesRead">number of byets read</param>
        public void OnRecieve(int bytesRead)
        {
            if (MemoryStreamRead == null)
            {
                MemoryStreamRead = new MemoryStream(ReadBuffer);
                MessageReader = new BinaryReader(MemoryStreamRead);
            }
            MemoryStreamRead.Seek(0, SeekOrigin.Begin);

            int dataLeftToRecieve = bytesRead;
            int currentReadIndex = 0;

            while (dataLeftToRecieve >= 2)
            {
                int nextPacketLength = BitConverter.ToUInt16(ReadBuffer, currentReadIndex);
                if (nextPacketLength == 0)
                    break;
                if (dataLeftToRecieve >= nextPacketLength)
                {
                    long position = MemoryStreamRead.Position;
                    GetData(currentReadIndex + 2, nextPacketLength - 2);
                    MemoryStreamRead.Position = position + nextPacketLength;
                    dataLeftToRecieve -= nextPacketLength;
                    currentReadIndex += nextPacketLength;
                    continue;
                }
                break;
            }
        }

        /// <summary>
        /// Simple update loop
        /// </summary>
        public async Task Update()
        {
            if (IsInWorld)
            {
                // This can bypass some anti-cheats that attempt to block headless clients
                if ((int)(DateTime.Now - (DateTime)Settings.LastSyncPeriod).TotalMilliseconds > Settings.AutoSyncPeriod)
                {
                    if (Settings.AutoSyncPlayerControl)
                    {
                        SendData(MessageID.PlayerControls, myPlayer);
                    }
                    if (Settings.AutoSyncPlayerZone)
                    {
                        SendData(MessageID.SyncPlayerZone, myPlayer);
                    }
                    if (Settings.AutoSyncPlayerLife)
                    {
                        SendData(MessageID.PlayerLife, myPlayer);
                    }
                    Settings.LastSyncPeriod = DateTime.Now;
                }
            }
            OnUpdate?.Invoke(this);
        }

        /// <summary>
        /// Disconnects the client from the server
        /// </summary>
        public async void Disconnect()
        {
            if (Settings.PrintAnyOutput && Settings.PrintDisconnectMessage)
            {
                Console.WriteLine($"Disconnected from world {World.CurrentWorld?.worldName}");
            }

            TCPClient.Exit = true;

            await TCPClient.ClientLoop;

            try
            {
                TCPClient.client.Shutdown(SocketShutdown.Both);
            } catch { }
            try
            {
                TCPClient.client.Close();
                TCPClient = null;
                Settings = null;
            } catch { }
            try
            {
                MemoryStreamWrite.Close();
            } catch { }
            try
            {
                MemoryStreamRead.Close();
            } catch { }
        }

        
        /// <summary>
        /// Handler for a packet being received
        /// </summary>
        /// <param name="start">position of the first byte of the packet in ReadBuffer</param>
        /// <param name="length">length of the packet</param>
        public void GetData(int start, int length)
        {
            if (TCPClient == null)
            {
                return;
            }
            BinaryReader reader = MessageReader;

            MessageReader.BaseStream.Position = start;

            byte messageType = reader.ReadByte();

            if (NetMessageReceived != null)
            {
                RawIncomingPacket packet = new RawIncomingPacket
                {
                    ReadBuffer = ReadBuffer,
                    Reader = reader,
                    MessageType = messageType,
                    ContinueWithPacket = true
                };

                NetMessageReceived?.Invoke(this, packet);

                if (!packet.ContinueWithPacket)
                {
                    return;
                }
            }

            switch (messageType)
            {
                case MessageID.PlayerInfo:
                {
                    int playerIndex = reader.ReadByte();
                    bool ServerWantsToRunCheckBytesInClientLoopThread = reader.ReadBoolean();

                    if (Settings.PrintAnyOutput && Settings.PrintPlayerId)
                    {
                        Console.WriteLine($"Player id of {playerIndex}");
                    }

                    if (myPlayer != playerIndex)
                    {
                        // Swap players
                        World.Players[playerIndex] = World.Players[myPlayer].Clone();
                        World.Players[myPlayer].Reset();

                        World.Players[playerIndex].whoAmI = playerIndex;
                        World.Players[myPlayer].whoAmI = myPlayer;
                        myPlayer = playerIndex;
                    }

                    LocalPlayer.active = true;

                    SendData(MessageID.SyncPlayer, playerIndex);
                    SendData(MessageID.ClientUUID, playerIndex);
                    SendData(MessageID.PlayerLife, playerIndex);
                    SendData(MessageID.PlayerMana, playerIndex);
                    SendData(MessageID.SyncPlayerBuffs, playerIndex);

                    for (int i = 0; i < 260; i++)
                    {
                        SendData(MessageID.SyncEquipment, playerIndex, i);
                    }

                    ConnectionState = ConnectionState.RequestingWorldData;

                    if (Settings.PrintAnyOutput && Settings.PrintWorldJoinMessages)
                    {
                        Console.WriteLine("Requesting world data");
                    }

                    SendData(MessageID.RequestWorldData);
                    break;
                }
                case MessageID.NetModules:
                {
                    ushort netModule = reader.ReadUInt16();
                    switch (netModule)
                    {
                        case NetModuleID.Liquid:
                        {
                            int liquidUpdateCount = MessageReader.ReadUInt16();
                            for (int i = 0; i < liquidUpdateCount; i++)
                            {
                                int num2 = MessageReader.ReadInt32();
                                byte liquid = MessageReader.ReadByte();
                                byte liquidType = MessageReader.ReadByte();
                                //Tile tile = Main.tile[num3, num4];
                                //if (tile != null)
                                //{
                                //    tile.liquid = liquid;
                                //    tile.liquidType(liquidType);
                                //}
                            }
                            break;
                        }
                        case NetModuleID.Text:
                        {
                            int authorIndex = reader.ReadByte();
                            NetworkText networkText = NetworkText.Deserialize(reader);
                            ChatMessageRecieved?.Invoke(this, new ChatMessage(authorIndex, networkText.ToString()));
                            break;
                        }
                    }
                    break;
                }
                case MessageID.WorldData:
                {
                    World.CurrentWorld.time = reader.ReadInt32();
                    BitsByte bitsByte20 = reader.ReadByte();
                    World.CurrentWorld.dayTime = bitsByte20[0];
                    World.CurrentWorld.bloodMoon = bitsByte20[1];
                    World.CurrentWorld.eclipse = bitsByte20[2];
                    World.CurrentWorld.moonPhase = reader.ReadByte();
                    World.CurrentWorld.maxTilesX = reader.ReadInt16();
                    World.CurrentWorld.maxTilesY = reader.ReadInt16();
                    World.CurrentWorld.spawnTileX = reader.ReadInt16();
                    World.CurrentWorld.spawnTileY = reader.ReadInt16();
                    World.CurrentWorld.worldSurface = reader.ReadInt16();
                    World.CurrentWorld.rockLayer = reader.ReadInt16();
                    World.CurrentWorld.worldID = reader.ReadInt32();
                    World.CurrentWorld.worldName = reader.ReadString();
                    World.CurrentWorld.GameMode = reader.ReadByte();
                    World.CurrentWorld.worldUUID = new Guid(reader.ReadBytes(16));
                    World.CurrentWorld.worldGenVer = reader.ReadUInt64();
                    World.CurrentWorld.moonType = reader.ReadByte();

                    // World Background 0
                    reader.ReadByte();
                    // World Background 1
                    reader.ReadByte();
                    // World Background 1
                    reader.ReadByte();
                    // World Background 1
                    reader.ReadByte();
                    // World Background 1
                    reader.ReadByte();
                    // World Background 2
                    reader.ReadByte();
                    // World Background 3
                    reader.ReadByte();
                    // World Background 4
                    reader.ReadByte();
                    // World Background 5
                    reader.ReadByte();
                    // World Background 6
                    reader.ReadByte();
                    // World Background 7
                    reader.ReadByte();
                    // World Background 8
                    reader.ReadByte();
                    // World Background 9
                    reader.ReadByte();

                    World.CurrentWorld.iceBackStyle = reader.ReadByte();
                    World.CurrentWorld.jungleBackStyle = reader.ReadByte();
                    World.CurrentWorld.hellBackStyle = reader.ReadByte();
                    World.CurrentWorld.windSpeedTarget = reader.ReadSingle();
                    World.CurrentWorld.numClouds = reader.ReadByte();

                    for (int i = 0; i < 3; i++)
                    {
                        // treeX[i]
                        reader.ReadInt32();
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        // treeStyle[i]
                        reader.ReadByte();
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        // caveBackX[i]
                        reader.ReadInt32();
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        // caveBackStyle[i]
                        reader.ReadByte();
                    }

                    for (int i = 0; i < 13; i++)
                    {
                        // some tree variation doodoo
                        reader.ReadByte();
                    }

                    World.CurrentWorld.maxRaining = reader.ReadSingle();
                    World.CurrentWorld.raining = World.CurrentWorld.maxRaining > 0f;

                    BitsByte bitsByte21 = reader.ReadByte();
                    World.CurrentWorld.shadowOrbSmashed = bitsByte21[0];
                    World.CurrentWorld.downedBoss1 = bitsByte21[1];
                    World.CurrentWorld.downedBoss2 = bitsByte21[2];
                    World.CurrentWorld.downedBoss3 = bitsByte21[3];
                    World.CurrentWorld.hardMode = bitsByte21[4];
                    World.CurrentWorld.downedClown = bitsByte21[5];
                    ServerSideCharacter = bitsByte21[6];
                    World.CurrentWorld.downedPlantBoss = bitsByte21[7];
                    //if (Main.ServerSideCharacter)
                    //{
                    //    Main.ActivePlayerFileData.MarkAsServerSide();
                    //}
                    BitsByte bitsByte22 = reader.ReadByte();
                    World.CurrentWorld.downedMechBoss1 = bitsByte22[0];
                    World.CurrentWorld.downedMechBoss2 = bitsByte22[1];
                    World.CurrentWorld.downedMechBoss3 = bitsByte22[2];
                    World.CurrentWorld.downedMechBossAny = bitsByte22[3];
                    World.CurrentWorld.cloudBGActive = (bitsByte22[4] ? 1 : 0);
                    World.CurrentWorld.crimson = bitsByte22[5];
                    World.CurrentWorld.pumpkinMoon = bitsByte22[6];
                    World.CurrentWorld.snowMoon = bitsByte22[7];
                    BitsByte bitsByte23 = reader.ReadByte();
                    World.CurrentWorld.fastForwardTime = bitsByte23[1];
                    //UpdateTimeRate();
                    bool num265 = bitsByte23[2];
                    World.CurrentWorld.downedSlimeKing = bitsByte23[3];
                    World.CurrentWorld.downedQueenBee = bitsByte23[4];
                    World.CurrentWorld.downedFishron = bitsByte23[5];
                    World.CurrentWorld.downedMartians = bitsByte23[6];
                    World.CurrentWorld.downedAncientCultist = bitsByte23[7];
                    BitsByte bitsByte24 = reader.ReadByte();
                    World.CurrentWorld.downedMoonlord = bitsByte24[0];
                    World.CurrentWorld.downedHalloweenKing = bitsByte24[1];
                    World.CurrentWorld.downedHalloweenTree = bitsByte24[2];
                    World.CurrentWorld.downedChristmasIceQueen = bitsByte24[3];
                    World.CurrentWorld.downedChristmasSantank = bitsByte24[4];
                    World.CurrentWorld.downedChristmasTree = bitsByte24[5];
                    World.CurrentWorld.downedGolemBoss = bitsByte24[6];
                    World.CurrentWorld.BirthdayPartyManualParty = bitsByte24[7];
                    BitsByte bitsByte25 = reader.ReadByte();
                    World.CurrentWorld.downedPirates = bitsByte25[0];
                    World.CurrentWorld.downedFrost = bitsByte25[1];
                    World.CurrentWorld.downedGoblins = bitsByte25[2];
                    World.CurrentWorld.Sandstorm.Happening = bitsByte25[3];
                    World.CurrentWorld.DD2.Ongoing = bitsByte25[4];
                    World.CurrentWorld.DD2.DownedInvasionT1 = bitsByte25[5];
                    World.CurrentWorld.DD2.DownedInvasionT2 = bitsByte25[6];
                    World.CurrentWorld.DD2.DownedInvasionT3 = bitsByte25[7];
                    BitsByte bitsByte26 = reader.ReadByte();
                    World.CurrentWorld.combatBookWasUsed = bitsByte26[0];
                    World.CurrentWorld.LanternNightManualLanterns = bitsByte26[1];
                    World.CurrentWorld.downedTowerSolar = bitsByte26[2];
                    World.CurrentWorld.downedTowerVortex = bitsByte26[3];
                    World.CurrentWorld.downedTowerNebula = bitsByte26[4];
                    World.CurrentWorld.downedTowerStardust = bitsByte26[5];
                    World.CurrentWorld.forceHalloweenForToday = bitsByte26[6];
                    World.CurrentWorld.forceXMasForToday = bitsByte26[7];
                    BitsByte bitsByte27 = reader.ReadByte();
                    World.CurrentWorld.boughtCat = bitsByte27[0];
                    World.CurrentWorld.boughtDog = bitsByte27[1];
                    World.CurrentWorld.boughtBunny = bitsByte27[2];
                    World.CurrentWorld.freeCake = bitsByte27[3];
                    World.CurrentWorld.drunkWorld = bitsByte27[4];
                    World.CurrentWorld.downedEmpressOfLight = bitsByte27[5];
                    World.CurrentWorld.downedQueenSlime = bitsByte27[6];
                    World.CurrentWorld.getGoodWorld = bitsByte27[7];
                    BitsByte bitsByte28 = reader.ReadByte();
                    World.CurrentWorld.tenthAnniversaryWorld = bitsByte28[0];
                    World.CurrentWorld.dontStarveWorld = bitsByte28[1];
                    World.CurrentWorld.downedDeerclops = bitsByte28[2];
                    World.CurrentWorld.notTheBeesWorld = bitsByte28[3];
                    World.CurrentWorld.SavedOreTiers_Copper = reader.ReadInt16();
                    World.CurrentWorld.SavedOreTiers_Iron = reader.ReadInt16();
                    World.CurrentWorld.SavedOreTiers_Silver = reader.ReadInt16();
                    World.CurrentWorld.SavedOreTiers_Gold = reader.ReadInt16();
                    World.CurrentWorld.SavedOreTiers_Cobalt = reader.ReadInt16();
                    World.CurrentWorld.SavedOreTiers_Mythril = reader.ReadInt16();
                    World.CurrentWorld.SavedOreTiers_Adamantite = reader.ReadInt16();
                    if (num265)
                    {
                        //Main.StartSlimeRain();
                    }
                    else
                    {
                        //Main.StopSlimeRain();
                    }
                    World.CurrentWorld.invasionType = reader.ReadSByte();
                    LobbyId = reader.ReadUInt64();
                    World.CurrentWorld.Sandstorm.IntendedSeverity = reader.ReadSingle();



                    if (ConnectionState == ConnectionState.RequestingWorldData)
                    {
                        if (Settings.PrintAnyOutput && Settings.PrintWorldJoinMessages)
                        {
                            Console.WriteLine($"Joining world \"{World.CurrentWorld.worldName}\"");
                        }

                        ConnectionState = ConnectionState.RequestingTileData;

                        World.CurrentWorld.SetupTiles(Settings.LoadTileSections);
                        WorldDataRecieved?.Invoke(this);

                        LocalPlayer.position = new Vector2(World.CurrentWorld.spawnTileX * 16f, World.CurrentWorld.spawnTileY * 16f);

                        SendData(MessageID.SpawnTileData, World.CurrentWorld.spawnTileX, World.CurrentWorld.spawnTileY);
                    }
                    break;
                }
                case MessageID.CompleteConnectionAndSpawn:
                {
                    if (Settings.SpawnPlayer)
                    {
                        SendData(MessageID.PlayerSpawn, myPlayer, 1);

                        for (int i = 0; i < 40; i++)
                        {
                            SendData(MessageID.SyncEquipment, myPlayer, i);
                        }

                        SendData(MessageID.SyncPlayerZone, myPlayer);
                        SendData(MessageID.PlayerControls, myPlayer);
                        SendData(MessageID.ClientSyncedInventory, myPlayer);
                    }
                    IsInWorld = true;
                    ClientConnectionCompleted?.Invoke(this);

                    ConnectionState = ConnectionState.Connected;
                    break;
                }
                case MessageID.FinishedConnectingToServer:
                {
                    FinishedConnectingToServer?.Invoke(this);
                    break;
                }
                case MessageID.Kick:
                {
                    IsInWorld = false;
                    if (Settings.PrintAnyOutput && Settings.PrintKickMessage)
                    {
                        Console.WriteLine($"Kicked from world {World.CurrentWorld?.worldName}");
                    }
                    break;
                }
                case MessageID.StatusText:
                {
                    int statusMax = reader.ReadInt32();
                    NetworkText statusText = NetworkText.Deserialize(reader);
                    byte flags = reader.ReadByte();
                    break;
                }
                case MessageID.NPCKillCountDeathTally:
                {
                    short npcType = reader.ReadInt16();
                    int npcKillCount = reader.ReadInt32();
                    break;
                }
                case MessageID.TileSection:
                {
                    // well documented code btw
                    // 🤨 📸
                    // 🤨 📸
                    // 🤨 📸

                    if (!Settings.IgnoreTileChunks)
                    {
                        World.CurrentWorld.DecompressTileSection(ReadBuffer, start + 1, length, Settings.LoadTileSections);
                    }
                    break;
                }
                case MessageID.TileManipulation:
                {
                    byte action = reader.ReadByte();
                    int tileX = reader.ReadInt16();
                    int tileY = reader.ReadInt16();
                    int flags = reader.ReadInt16();
                    int flags2 = reader.ReadByte();

                    bool? handleManpipulation = TileManipulationMessageRecieved?.Invoke(this, new TileManipulation(action, tileX, tileY, flags, flags2));
                    if (!handleManpipulation.HasValue || handleManpipulation.Value)
                    {
                        TileManipulationHandler.Handle(this, action, tileX, tileY, flags, flags2);
                    }
                    break;
                }
                case MessageID.SyncPlayer:
                {
                    byte whoAreThey = reader.ReadByte();

                    if (whoAreThey == myPlayer && !ServerSideCharacter)
                    {
                        break;
                    }

                    // skin variant
                    World.Players[whoAreThey].skinVariant = reader.ReadByte();

                    // hair
                    World.Players[whoAreThey].hairType = reader.ReadByte();

                    World.Players[whoAreThey].name = reader.ReadString();

                    // hair dye
                    World.Players[whoAreThey].hairDye = reader.ReadByte();

                    // accessory/armor visibility 1
                    BitsByte hideVisibleAccessory = reader.ReadByte();

                    // accessory/armor visibility 2
                    BitsByte hideVisibleAccessory2 = reader.ReadByte();

                    // hide misc
                    reader.ReadByte();

                    // hairColor
                    World.Players[whoAreThey].hairColor = reader.ReadRGB();

                    // skinColor
                    World.Players[whoAreThey].skinColor = reader.ReadRGB();

                    // eyeColor
                    World.Players[whoAreThey].eyeColor = reader.ReadRGB();

                    // shirtColor
                    World.Players[whoAreThey].shirtColor = reader.ReadRGB();

                    // underShirtColor
                    World.Players[whoAreThey].underShirtColor = reader.ReadRGB();

                    // pantsColor
                    World.Players[whoAreThey].pantsColor = reader.ReadRGB();

                    // shoeColor
                    World.Players[whoAreThey].shoeColor = reader.ReadRGB();

                    BitsByte bitsByte7 = reader.ReadByte();

                    BitsByte bitsByte8 = reader.ReadByte();

                    break;
                }
                case MessageID.PlayerActive:
                {
                    byte whoAreThey = reader.ReadByte();
                    bool active = reader.ReadByte() == 1;

                    World.Players[whoAreThey].active = active;
                    break;
                }
                case MessageID.PlayerControls:
                {
                    byte whoAreThey = reader.ReadByte();

                    if (whoAreThey != myPlayer || ServerSideCharacter)
                    {
                        Player plr = World.Players[whoAreThey];

                        BitsByte control = reader.ReadByte();
                        BitsByte pulley = reader.ReadByte();
                        BitsByte sitting = reader.ReadByte();
                        BitsByte what = reader.ReadByte();
                        plr.controlUp = control[0];
                        plr.controlDown = control[1];
                        plr.controlLeft = control[2];
                        plr.controlRight = control[3];
                        plr.controlJump = control[4];
                        plr.controlUseItem = control[5];
                        plr.direction = (control[6] ? 1 : (-1));
                        if (pulley[0])
                        {
                            plr.pulley = true;
                            plr.pulleyDir = (byte)((!pulley[1]) ? 1u : 2u);
                        }
                        else
                        {
                            plr.pulley = false;
                        }
                        plr.vortexStealthActive = pulley[3];
                        plr.gravDir = (pulley[4] ? 1 : (-1));
                        //plr.TryTogglingShield(bitsByte15[5]);
                        plr.ghost = pulley[6];
                        plr.selectedItem = reader.ReadByte();
                        plr.position = reader.ReadVector2();
                        if (pulley[2])
                        {
                            plr.velocity = reader.ReadVector2();
                        }
                        else
                        {
                            plr.velocity = Vector2.Zero;
                        }
                        if (sitting[6])
                        {
                            plr.PotionOfReturnOriginalUsePosition = reader.ReadVector2();
                            plr.PotionOfReturnHomePosition = reader.ReadVector2();
                        }
                        else
                        {
                            plr.PotionOfReturnOriginalUsePosition = null;
                            plr.PotionOfReturnHomePosition = null;
                        }
                        plr.tryKeepingHoveringUp = sitting[0];
                        plr.IsVoidVaultEnabled = sitting[1];
                        plr.isSitting = sitting[2];
                        plr.downedDD2EventAnyDifficulty = sitting[3];
                        plr.isPettingAnimal = sitting[4];
                        plr.isTheAnimalBeingPetSmall = sitting[5];
                        plr.tryKeepingHoveringDown = sitting[7];
                    }
                    break;
                }
                case MessageID.PlayerLife:
                {
                    byte whoAreThey = reader.ReadByte();

                    if (whoAreThey != myPlayer || ServerSideCharacter)
                    {
                        Player plr = World.Players[whoAreThey];

                        plr.statLife = reader.ReadInt16();
                        plr.statLifeMax = reader.ReadInt16();
                        if (plr.statLifeMax < 100)
                        {
                            plr.statLifeMax = 100;
                        }
                    }
                    break;
                }
                case MessageID.PlayerMana:
                {
                    byte whoAreThey = reader.ReadByte();
                    Player plr = World.Players[whoAreThey];
                    if (myPlayer == whoAreThey && !ServerSideCharacter)
                    {
                        break;
                    }
                    plr.statMana = reader.ReadInt16();
                    plr.statManaMax = reader.ReadInt16();
                    break;
                }
                case MessageID.SyncNPC:
                {
                    int npcIndex = reader.ReadInt16();
                    break;
                }
                case MessageID.ReleaseItemOwnership:
                {
                    int itemIndex = reader.ReadInt16();
                    SendData(MessageID.ItemOwner, itemIndex);
                    break;
                }
                case MessageID.SyncProjectile:
                {
                    break;
                }
                case MessageID.KillProjectile:
                {
                    break;
                }
                case MessageID.SyncEquipment:
                {
                    byte whoAreThey = reader.ReadByte();

                    if (whoAreThey == myPlayer && !ServerSideCharacter/* && !Main.player[whoAreThey].HasLockedInventory() <--- can someone please explain what this is for?*/)
                    {
                        break;
                    }

                    Player plr = World.Players[whoAreThey];

                    lock (plr)
                    {
                        short inventorySlot = reader.ReadInt16();
                        Item item = plr.inventory[inventorySlot];

                        short stack = reader.ReadInt16();
                        byte prefix = reader.ReadByte();
                        short type = reader.ReadInt16();

                        if (item == null)
                        {
                            item = new Item(type, stack, prefix);
                        }
                        else
                        {
                            item.type = type;
                            item.prefix = prefix;
                            item.stack = stack;
                        }
                        plr.inventory[inventorySlot] = item;
                    }
                    break;
                }
                case MessageID.InstancedItem:
                case MessageID.SyncItem:
                {
                    int itemId = reader.ReadInt16();
                    Vector2 itemPosition = reader.ReadVector2();
                    Vector2 itemVelocity = reader.ReadVector2();
                    int itemStack = reader.ReadInt16();
                    int itemPrefix = reader.ReadByte();
                    int noDelay = reader.ReadByte();
                    int netID = reader.ReadInt16();

                    if (netID == 0)
                    {
                        World.Items[itemId].active = false;
                        break;
                    }

                    Item item2 = World.Items[itemId];
                    item2.SetTypeFromNetID(netID);
                    item2.prefix = itemPrefix;
                    item2.stack = itemStack;
                    item2.position = itemPosition;
                    item2.velocity = itemVelocity;
                    item2.active = true;
                    if (messageType == 90)
                    {

                        // the item is only on our client, which is cringe as fuck
                        item2.instanced = true;
                        item2.whoIsThisInstancedItemFor = myPlayer;

                        // what do i even do for this
                        // stays around for 10 seconds? this is cringe.
                        // item2.keepTime = 600;
                    }
                    break;
                }
                case MessageID.SyncPlayerItemRotation:
                {
                    byte whoAreThey = reader.ReadByte();

                    float rotation = reader.ReadSingle();
                    short animation = reader.ReadInt16();
                    break;
                }
                case MessageID.UpdateSign:
                {
                    int signId = reader.ReadInt16();
                    int x = reader.ReadInt16();
                    int y = reader.ReadInt16();
                    string text = reader.ReadString();
                    byte whoDidThis = reader.ReadByte();
                    byte signFlags = reader.ReadByte();

                    if (signId >= 0 && signId < 1000)
                    {
                        if (World.CurrentWorld.Signs[signId] == null)
                        {
                            World.CurrentWorld.Signs[signId] = new Sign();
                        }
                        World.CurrentWorld.Signs[signId].text = text;
                        World.CurrentWorld.Signs[signId].x = x;
                        World.CurrentWorld.Signs[signId].y = y;
                    }
                    break;
                }
                case MessageID.ChestName:
                {
                    int chestId = reader.ReadInt16();
                    int chestX = reader.ReadInt16();
                    int chestY = reader.ReadInt16();
                    if (chestId >= 0 && chestId < 8000)
                    {
                        Chest chest3 = World.CurrentWorld.Chests[chestId];
                        if (chest3 == null)
                        {
                            chest3 = new Chest();
                            chest3.x = chestX;
                            chest3.y = chestY;
                            World.CurrentWorld.Chests[chestId] = chest3;
                        }
                        else if (chest3.x != chestX || chest3.y != chestY)
                        {
                            break;
                        }
                        chest3.Name = reader.ReadString();
                    }
                    break;
                }
                case MessageID.PlaceChest:
                {
                    int action = reader.ReadByte();
                    int chestX = reader.ReadInt16();
                    int chestY = reader.ReadInt16();
                    int style = reader.ReadInt16();
                    int chestId = reader.ReadInt16();

                    switch (action)
                    {
                        case 0:
                            if (chestId == -1)
                            {
                                World.CurrentWorld.KillTile(chestX, chestY);
                                break;
                            }
                            World.CurrentWorld.PlaceChestDirect(chestX, chestY, 21, style, chestId);
                            break;
                        case 2:
                            if (chestId == -1)
                            {
                                World.CurrentWorld.KillTile(chestX, chestY);
                                break;
                            }
                            World.CurrentWorld.PlaceDresserDirect(chestX, chestY, 88, style, chestId);
                            break;
                        case 4:
                            if (chestId == -1)
                            {
                                World.CurrentWorld.KillTile(chestX, chestY);
                                break;
                            }
                            World.CurrentWorld.PlaceChestDirect(chestX, chestY, 467, style, chestId);
                            break;
                        default:
                            World.CurrentWorld.KillChestDirect(chestX, chestY, chestId);
                            World.CurrentWorld.KillTile(chestX, chestY);
                            break;
                    }
                    break;
                }
                default:
                    if (Settings.PrintAnyOutput && Settings.PrintUnknownPackets)
                    {
                        Console.WriteLine($"Recieved unknown message of type {MessageID.GetName(messageType)}");
                    }
                    break;
            }
        }

        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="messageType">type of message to be sent</param>
        public void SendData(int messageType, int number = 0, float number2 = 0, float number3 = 0, float number4 = 0, int number5 = 0)
        {
            if (TCPClient == null)
                return;
            lock (WriteBuffer)
            {
                BinaryWriter writer = MessageWriter;

                writer.Seek(2, SeekOrigin.Begin);

                writer.Write((byte)messageType);
                switch (messageType)
                {
                    case MessageID.Hello:
                        writer.Write("Terraria" + VersionNumber.ToString());
                        break;
                    case MessageID.SyncPlayer:
                    {
                        Player plr = World.Players[number];


                        writer.Write((byte)number);

                        // skin variant
                        writer.Write((byte)plr.skinVariant);

                        // hair
                        writer.Write((byte)plr.hairType);

                        // name
                        writer.Write(plr.name);

                        // hair dye
                        writer.Write((byte)plr.hairDye);

                        // accessory/armor visibility 1
                        BitsByte hideVisibleAccessory = (byte)0;
                        writer.Write(hideVisibleAccessory);

                        // accessory/armor visibility 2
                        hideVisibleAccessory = (byte)0;
                        writer.Write(hideVisibleAccessory);

                        // hide misc
                        writer.Write((byte)0);

                        // hairColor
                        writer.WriteRGB(plr.hairColor);

                        // skinColor
                        writer.WriteRGB(plr.skinColor);

                        // eyeColor
                        writer.WriteRGB(plr.eyeColor);

                        // shirtColor
                        writer.WriteRGB(plr.shirtColor);

                        // underShirtColor
                        writer.WriteRGB(plr.underShirtColor);

                        // pantsColor
                        writer.WriteRGB(plr.pantsColor);

                        // shoeColor
                        writer.WriteRGB(plr.shoeColor);

                        BitsByte bitsByte7 = (byte)0;
                        if (plr.difficulty == 1)
                        {
                            bitsByte7[0] = true;
                        }
                        else if (plr.difficulty == 2)
                        {
                            bitsByte7[1] = true;
                        }
                        else if (plr.difficulty == 3)
                        {
                            bitsByte7[3] = true;
                        }

                        // plr.extraAccessory;
                        bitsByte7[2] = plr.extraAccessory;
                        writer.Write(bitsByte7);

                        BitsByte bitsByte8 = (byte)0;
                        //plr.UsingBiomeTorches;
                        bitsByte8[0] = false;
                        //plr.happyFunTorchTime;
                        bitsByte8[1] = false;
                        //plr.unlockedBiomeTorches;
                        bitsByte8[2] = false;
                        writer.Write(bitsByte8);
                    }
                    break;
                    case MessageID.ClientUUID:
                        writer.Write(clientUUID);
                        break;
                    case MessageID.PlayerLife:
                    {
                        Player plr = World.Players[number];
                        writer.Write((byte)number);
                        //statLife
                        writer.Write((short)plr.statLife);
                        //statLifeMax
                        writer.Write((short)plr.statLifeMax);
                        break;
                    }
                    case MessageID.PlayerMana:
                    {
                        Player plr = World.Players[number];
                        writer.Write((byte)number);
                        //statMana
                        writer.Write((short)plr.statMana);
                        //statManaMax
                        writer.Write((short)plr.statManaMax);
                        break;
                    }
                    case MessageID.SyncPlayerBuffs:
                    {
                        writer.Write((byte)number);
                        for (int n = 0; n < 22; n++)
                        {
                            // buffType[n]
                            writer.Write((ushort)0);
                        }
                        break;
                    }
                    case MessageID.RequestWorldData:
                        break;
                    case MessageID.SyncEquipment:
                    {
                        Player plr = World.Players[number];
                        Item item = plr.inventory[(int)number2];
                        // player index
                        writer.Write((byte)number);

                        // inventory index
                        writer.Write((short)number2);

                        // stack?
                        writer.Write((short)item.stack);
                        // prefix?
                        writer.Write((byte)item.prefix);
                        // type?
                        writer.Write((short)item.type);
                    }
                    break;
                    case MessageID.SpawnTileData:
                        writer.Write((int)number);
                        writer.Write((int)number2);
                        break;
                    case MessageID.PlayerSpawn:
                    {
                        writer.Write((byte)number);
                        writer.Write((short)World.CurrentWorld.spawnTileX);
                        writer.Write((short)World.CurrentWorld.spawnTileY);
                        writer.Write(0);
                        writer.Write((byte)number2);
                        break;
                    }
                    case MessageID.PlayerControls:
                    {
                        Player plr = World.Players[number];
                        writer.Write((byte)number);
                        // Control, b3 must be true to send velocity
                        writer.Write(new BitsByte(b2: true));
                        // Pulley
                        writer.Write((byte)0);
                        // Misc
                        writer.Write((byte)0);
                        // SleepingInfo
                        writer.Write((byte)0);
                        // Selected Item
                        writer.Write((byte)0);

                        writer.Write(plr.position);

                        writer.Write(plr.velocity);

                        break;
                    }
                    case MessageID.SyncPlayerZone:
                    {
                        writer.Write((byte)number);
                        writer.Write((byte)0);
                        writer.Write((byte)0);
                        writer.Write((byte)0);
                        writer.Write((byte)0);
                        break;
                    }
                    case MessageID.ItemOwner:
                    {
                        writer.Write((short)number);
                        writer.Write((short)255);
                        break;
                    }
                    case MessageID.TileManipulation:
                    {
                        // TileManipulationID
                        writer.Write((byte)number);
                        // tileX
                        writer.Write((short)number2);
                        // tileY
                        writer.Write((short)number3);
                        // flags 1
                        writer.Write((short)number4);
                        // flags 2
                        writer.Write((byte)number5);
                        break;
                    }
                    case MessageID.PaintTile:
                    case MessageID.PaintWall:
                    {
                        writer.Write((short)number);
                        writer.Write((short)number2);
                        writer.Write((byte)number3);
                        break;
                    };
                    case MessageID.SyncItem:
                    {
                        Item item7 = World.Items[number];
                        writer.Write((short)number);
                        writer.Write(item7.position);
                        writer.Write(item7.velocity);
                        writer.Write((short)item7.stack);
                        writer.Write(item7.prefix);
                        writer.Write((byte)number2);
                        short value5 = 0;
                        if (item7.active && item7.stack > 0)
                        {
                            value5 = (short)item7.GetNetID();
                        }
                        writer.Write(value5);
                        break;
                    }
                }

                int length = (int)MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);


                if (NetMessageSent != null)
                {
                    RawOutgoingPacket packet = new RawOutgoingPacket
                    {
                        WriteBuffer = WriteBuffer,
                        Writer = writer,
                        MessageType = messageType,
                        ContinueWithPacket = true
                    };

                    NetMessageSent?.Invoke(this, packet);

                    if (!packet.ContinueWithPacket)
                    {
                        return;
                    }
                }
                TCPClient.Send(WriteBuffer, length);
            }
        }
    }
}