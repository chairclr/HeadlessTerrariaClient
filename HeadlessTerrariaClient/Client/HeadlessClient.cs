using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Util;
using System.Net.Sockets;
using System.Numerics;

// Much of packet documentation is from Terraria's NetMessage.cs and from https://tshock.readme.io/docs/multiplayer-packet-structure
namespace HeadlessTerrariaClient.Client
{
    public class HeadlessClient
    {
        public ArkTCPClient TCPClient;
        public byte[] WriteBuffer = new byte[131070];
        public byte[] ReadBuffer = new byte[131070];
        public MemoryStream MemoryStreamWrite;
        public BinaryWriter MessageWriter;
        public MemoryStream MemoryStreamRead;
        public BinaryReader MessageReader;
        public delegate void OnSomethingHappened(HeadlessClient hc);
        public delegate void OnSomethingHappened<T>(HeadlessClient hc, T e);
        public delegate void OnSomethingHappened<T1, T2>(HeadlessClient hc, T1 e, T2 e2);
        public delegate void OnSomethingHappened<T1, T2, T3>(HeadlessClient hc, T1 e, T2 e2, T3 e3);
        public OnSomethingHappened WorldDataRecieved = null;
        public OnSomethingHappened FinishedConnectingToServer = null;
        public OnSomethingHappened ClientConnectionCompleted = null;
        public OnSomethingHappened OnUpdate = null;
        public OnSomethingHappened<ChatMessage> ChatMessageRecieved = null;
        public OnSomethingHappened<TileManipulation> TileManipulationMessageRecieved = null;
        public OnSomethingHappened<RawIncomingPacket> NetMessageRecieved = null;
        public OnSomethingHappened<RawOutgoingPacket> NetMessageSent = null;

        public dynamic Settings = new Settings();

        public HeadlessClient()
        {
            Settings.PrintAnyOutput = true;
            Settings.PrintPlayerId = true;
            Settings.PrintWorldJoinMessages = true;
            Settings.PrintConnectionMessages = true;
            Settings.PrintUnknownPackets = false;
            Settings.PrintKickMessage = true;
            Settings.PrintDisconnectMessage = true;
            Settings.SpawnPlayer = true;
            Settings.AwaitConnectToServerCall = true;
            Settings.RunGameLoop = true;
            Settings.AutoSyncPlayerZone = true;
            Settings.AutoSyncPlayerControl = false;
            Settings.AutoSyncPlayerLife = true;
            Settings.AutoSyncPeriod = 2000;
            Settings.LastSyncPeriod = DateTime.Now;
            Settings.UpdateTimeout = 200;
        }

        public async Task Connect(string address, short port)
        {
            if (!SetIP(address, out IPAddress ipAddress))
            {
                throw new ArgumentException($"Could not resolve ip {address}");
            }

            TCPClient = new ArkTCPClient(ipAddress, ReadBuffer, port, (x, y) => { OnRecieve(y).Wait(); });
            MemoryStreamWrite = new MemoryStream(WriteBuffer);
            MessageWriter = new BinaryWriter(MemoryStreamWrite);
            if (Settings.PrintAnyOutput && Settings.PrintConnectionMessages)
            {
                Console.WriteLine($"Connecting to {address}:{port}");
            }

            
            if (Settings.AwaitConnectToServerCall)
            {
                await ConnectToServer();
            }
            else
            {
                ConnectToServer();
            }

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

            await SendDataAsync(MessageID.Hello);
        }
        public bool SetIP(string remoteAddress, out IPAddress address)
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
        public async Task OnRecieve(int bytesRead)
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
                if (dataLeftToRecieve >= nextPacketLength)
                {
                    long position = MemoryStreamRead.Position;
                    await GetData(currentReadIndex + 2, nextPacketLength - 2);
                    MemoryStreamRead.Position = position + nextPacketLength;
                    dataLeftToRecieve -= nextPacketLength;
                    currentReadIndex += nextPacketLength;
                    continue;
                }
                break;
            }
        }

        public async Task Update()
        {
            if (IsInWorld)
            {
                // This can bypass some anti-cheats that attempt to block headless clients
                if ((int)(DateTime.Now - (DateTime)Settings.LastSyncPeriod).TotalMilliseconds > Settings.AutoSyncPeriod)
                {
                    if (Settings.AutoSyncPlayerControl)
                    {
                        await SendDataAsync(MessageID.PlayerControls, myPlayer);
                    }
                    if (Settings.AutoSyncPlayerZone)
                    {
                        await SendDataAsync(MessageID.SyncPlayerZone, myPlayer);
                    }
                    if (Settings.AutoSyncPlayerLife)
                    {
                        await SendDataAsync(MessageID.PlayerLife, myPlayer);
                    }
                    Settings.LastSyncPeriod = DateTime.Now;
                }
            }
            await OnUpdateAsync?.Invoke(this);
        }
        public void Disconnect()
        {
            if (Settings.PrintAnyOutput && Settings.PrintDisconnectMessage)
            {
                Console.WriteLine($"Disconnected from world {World.CurrentWorld?.worldName}");
            }

            try
            {
                TCPClient.client.Close();
            } catch { }
            TCPClient.Exit = true;
            try
            {
                UserData = null;
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

        public ClientWorld World;
        public int myPlayer;
        public Player LocalPlayer
        {
            get
            {
                return World.player[myPlayer];
            }
        }
        public string clientUUID;
        public bool ServerSideCharacter;
        public ulong LobbyId;
        public int VersionNumber = 248;
        public PlayerData PlayerFile = new PlayerData();
        public object UserData;
        private bool initalWorldData = false;
        public bool IsInWorld
        {
            get;
            private set;
        }

        public T GetUserData<T>()
        {
            return (T)UserData;
        }

        public async Task GetData(int start, int length)
        {
            if (TCPClient == null)
            {
                return;
            }
            BinaryReader reader = MessageReader;

            MessageReader.BaseStream.Position = start;

            byte messageType = reader.ReadByte();

            if (NetMessageRecievedAsync != null)
            {
                RawIncomingPacket packet = new RawIncomingPacket
                {
                    ReadBuffer = ReadBuffer,
                    Reader = reader,
                    MessageType = messageType,
                    ContinueWithPacket = true
                };

                NetMessageRecievedAsync?.Invoke(this, packet).Wait();

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

                    myPlayer = playerIndex;
                    LocalPlayer.active = true;
                    LocalPlayer.SyncDataWithTemp(PlayerFile);

                    await SendDataAsync(MessageID.SyncPlayer, playerIndex);
                    await SendDataAsync(MessageID.ClientUUID, playerIndex);
                    await SendDataAsync(MessageID.PlayerLife, playerIndex);
                    await SendDataAsync(MessageID.PlayerMana, playerIndex);
                    await SendDataAsync(MessageID.SyncPlayerBuffs, playerIndex);

                    for (int i = 0; i < 260; i++)
                    {
                        await SendDataAsync(MessageID.SyncEquipment, playerIndex, i);
                    }

                    if (Settings.PrintAnyOutput && Settings.PrintWorldJoinMessages)
                    {
                        Console.WriteLine("Requesting world data");
                    }
                    await SendDataAsync(MessageID.RequestWorldData);
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
                            await ChatMessageRecievedAsync?.Invoke(this, new ChatMessage(authorIndex, networkText.ToString()));
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

                    /*WorldGen.setBG(0, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(10,*/
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(11,*/
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(12,*/
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(1, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(2, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(3, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(4, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(5, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(6, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(7, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(8, */
                    reader.ReadByte()/*)*/;
                    /*WorldGen.setBG(9, */
                    reader.ReadByte()/*)*/;

                    World.CurrentWorld.iceBackStyle = reader.ReadByte();
                    World.CurrentWorld.jungleBackStyle = reader.ReadByte();
                    World.CurrentWorld.hellBackStyle = reader.ReadByte();
                    World.CurrentWorld.windSpeedTarget = reader.ReadSingle();
                    World.CurrentWorld.numClouds = reader.ReadByte();

                    for (int num261 = 0; num261 < 3; num261++)
                    {
                        /*Main.treeX[num261] = */
                        reader.ReadInt32();
                    }
                    for (int num262 = 0; num262 < 4; num262++)
                    {
                        /*Main.treeStyle[num262] = */
                        reader.ReadByte();
                    }
                    for (int num263 = 0; num263 < 3; num263++)
                    {
                        /*Main.caveBackX[num263] = */
                        reader.ReadInt32();
                    }
                    for (int num264 = 0; num264 < 4; num264++)
                    {
                        /*Main.caveBackStyle[num264] = */
                        reader.ReadByte();
                    }

                    for (int num696969 = 0; num696969 < 13; num696969++)
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

                    //CurrentWorld.tile = new Tile[CurrentWorld.maxTilesX,CurrentWorld.maxTilesY];


                    if (!initalWorldData)
                    {
                        initalWorldData = true;
                        if (Settings.PrintAnyOutput && Settings.PrintWorldJoinMessages)
                        {
                            Console.WriteLine($"Joining world \"{World.CurrentWorld.worldName}\"");
                        }

                        await WorldDataRecievedAsync?.Invoke(this);
                        LocalPlayer.position = new Vector2(World.CurrentWorld.spawnTileX * 16f, World.CurrentWorld.spawnTileY * 16f);
                        await SendDataAsync(MessageID.SpawnTileData, World.CurrentWorld.spawnTileX, World.CurrentWorld.spawnTileY);
                    }
                    break;
                }
                case MessageID.FinishedConnectingToServer:
                {
                    await FinishedConnectingToServerAsync?.Invoke(this);
                    break;
                }
                case MessageID.CompleteConnectionAndSpawn:
                {
                    if (Settings.SpawnPlayer)
                    {
                        await SendDataAsync(MessageID.PlayerSpawn, myPlayer, 1);

                        for (int i = 0; i < 40; i++)
                        {
                            await SendDataAsync(MessageID.SyncEquipment, myPlayer, i, 0, 0, 0);
                        }
                        await SendDataAsync(MessageID.SyncPlayerZone, myPlayer);
                        await SendDataAsync(MessageID.PlayerControls, myPlayer);
                        await SendDataAsync(MessageID.ClientSyncedInventory, myPlayer);
                    }
                    IsInWorld = true;
                    await ClientConnectionCompletedAsync?.Invoke(this);
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
                    World.CurrentWorld.DecompressTileSection(ReadBuffer, start, length);
                    break;
                }
                case MessageID.TileManipulation:
                {
                    byte action = reader.ReadByte();
                    int tileX = reader.ReadInt16();
                    int tileY = reader.ReadInt16();
                    int flags = reader.ReadInt16();
                    int flags2 = reader.ReadByte();

                    await TileManipulationMessageRecievedAsync?.Invoke(this, new TileManipulation(action, tileX, tileY, flags, flags2));
                    break;
                }
                case MessageID.SyncPlayer:
                {
                    byte whoAreThey = reader.ReadByte();


                    // skin variant
                    World.player[whoAreThey].skinVariant = reader.ReadByte();

                    // hair
                    reader.ReadByte();

                    World.player[whoAreThey].name = reader.ReadString();

                    // hair dye
                    World.player[whoAreThey].hairDye = reader.ReadByte();

                    // accessory/armor visibility 1
                    BitsByte hideVisibleAccessory = reader.ReadByte();

                    // accessory/armor visibility 2
                    BitsByte hideVisibleAccessory2 = reader.ReadByte();

                    // hide misc
                    reader.ReadByte();

                    // hairColor
                    World.player[whoAreThey].hairColor = reader.ReadRGB();

                    // skinColor
                    World.player[whoAreThey].skinColor = reader.ReadRGB();

                    // eyeColor
                    World.player[whoAreThey].eyeColor = reader.ReadRGB();

                    // shirtColor
                    World.player[whoAreThey].shirtColor = reader.ReadRGB();

                    // underShirtColor
                    World.player[whoAreThey].underShirtColor = reader.ReadRGB();

                    // pantsColor
                    World.player[whoAreThey].pantsColor = reader.ReadRGB();

                    // shoeColor
                    World.player[whoAreThey].shoeColor = reader.ReadRGB();

                    BitsByte bitsByte7 = reader.ReadByte();

                    BitsByte bitsByte8 = reader.ReadByte();

                    break;
                }
                case MessageID.PlayerActive:
                {
                    byte whoAreThey = reader.ReadByte();
                    bool active = reader.ReadByte() == 1;

                    World.player[whoAreThey].active = active;
                    break;
                }
                case MessageID.PlayerControls:
                {
                    byte whoAreThey = reader.ReadByte();

                    Player plr = World.player[whoAreThey];
                    BitsByte control = reader.ReadByte();
                    BitsByte pulley = reader.ReadByte();
                    BitsByte misc = reader.ReadByte();
                    byte sleeping = reader.ReadByte();
                    byte selectedItem = reader.ReadByte();
                    plr.position.X = reader.ReadSingle();
                    plr.position.Y = reader.ReadSingle();
                    //plr.velocity.X = reader.ReadSingle();
                    //plr.velocity.Y = reader.ReadSingle();
                    break;
                }
                case MessageID.PlayerLife:
                {
                    byte whoAreThey = reader.ReadByte();
                    Player plr = World.player[whoAreThey];

                    plr.statLife = reader.ReadInt16();
                    plr.statLifeMax = reader.ReadInt16();
                    break;
                }
                case MessageID.PlayerMana:
                {
                    byte whoAreThey = reader.ReadByte();
                    Player plr = World.player[whoAreThey];

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
                    await SendDataAsync(MessageID.ItemOwner, itemIndex);
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
                    Player plr = World.player[whoAreThey];
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
                    break;
                }
                case MessageID.SyncItem:
                {
                    break;
                }
                case MessageID.SyncPlayerItemRotation:
                {
                    byte whoAreThey = reader.ReadByte();

                    float rotation = reader.ReadSingle();
                    short animation = reader.ReadInt16();
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

        public async Task SendDataAsync(int messageType, int number = 0, float number2 = 0, float number3 = 0, float number4 = 0, int number5 = 0)
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
                        Player plr = World.player[number];
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
                        Player plr = World.player[number];
                        writer.Write((byte)number);
                        //statLife
                        writer.Write((short)plr.statLife);
                        //statLifeMax
                        writer.Write((short)plr.statLifeMax);
                        break;
                    }
                    case MessageID.PlayerMana:
                    {
                        Player plr = World.player[number];
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
                        Player plr = World.player[number];
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
                        Player plr = World.player[number];
                        writer.Write((byte)number);
                        // Control
                        writer.Write((byte)0);
                        // Pulley
                        writer.Write((byte)0);
                        // Misc
                        writer.Write((byte)0);
                        // SleepingInfo
                        writer.Write((byte)0);
                        // Selected Item
                        writer.Write((byte)0);

                        writer.Write(plr.position.X);
                        writer.Write(plr.position.Y);

                        writer.Write(plr.velocity.X);
                        writer.Write(plr.velocity.Y);

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
                }

                int length = (int)MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);


                if (NetMessageSentAsync != null)
                {
                    RawOutgoingPacket packet = new RawOutgoingPacket
                    {
                        WriteBuffer = WriteBuffer,
                        Writer = writer,
                        MessageType = messageType,
                        ContinueWithPacket = true
                    };

                    NetMessageSentAsync?.Invoke(this, packet).Wait();

                    if (!packet.ContinueWithPacket)
                    {
                        return;
                    }
                }
                // literally cringe lololol
                TCPClient.SendAsync(WriteBuffer, length);
            }
        }
    }
}