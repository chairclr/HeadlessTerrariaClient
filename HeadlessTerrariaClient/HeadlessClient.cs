using System;
using System.Drawing;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

// Much of packet documentation is from Terraria's NetMessage.cs and from https://tshock.readme.io/docs/multiplayer-packet-structure
namespace HeadlessTerrariaClient
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
        public OnSomethingHappened JoinedWorld = null;
        public OnSomethingHappened<ChatMessage> ChatMessageRecieved = null;

        public dynamic Settings = new Settings();

        public HeadlessClient()
        {
            Settings.PrintAnyOutput = true;
            Settings.PrintPlayerId = true;
            Settings.PrintWorldJoinMessages = true;
            Settings.PrintConnectionMessages = true;
            Settings.PrintUnknownPackets = false;
            Settings.SpawnPlayer = true;
        }

        public void Connect(string address, short port)
        {
            TCPClient = new ArkTCPClient(IPAddress.Parse(address), ReadBuffer, port, (x, y) => { return OnRecieve(y); });
            MemoryStreamWrite = new MemoryStream(WriteBuffer);
            MessageWriter = new BinaryWriter(MemoryStreamWrite);
            if (Settings.PrintAnyOutput && Settings.PrintConnectionMessages)
            {
                Console.WriteLine($"Connecting to {address}:{port}");
            }

            for (int i = 0; i < 255; i++)
            {
                player[i] = new Player();
                player[i].whoAmI = i;
                player[i].active = false;
                player[i].name = "";
            }

            ConnectToServer();
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

            SendData(MessageID.Hello);
        }
        public int OnRecieve(int bytesRead)
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
                    GetData(currentReadIndex + 2, nextPacketLength - 2, out var _);
                    MemoryStreamRead.Position = position + nextPacketLength;
                    dataLeftToRecieve -= nextPacketLength;
                    currentReadIndex += nextPacketLength;
                    continue;
                }
                if (dataLeftToRecieve == 2)
                    return BitConverter.ToUInt16(ReadBuffer, currentReadIndex);
                break;
            }

            return -1;
        }


        public Player[] player = new Player[256];
        public int myPlayer;
        public Player LocalPlayer
        {
            get
            {
                return player[myPlayer];
            }
        }
        public World CurrentWorld;
        public string clientUUID;
        public bool ServerSideCharacter;
        public ulong LobbyId;
        public int VersionNumber = 248;
        private bool joined = false;
        public PlayerData PlayerFile = new PlayerData();
        public object UserData;

        public T GetUserData<T>()
        {
            return (T)UserData;
        }

        public void GetData(int start, int length, out int msgType)
        {
            lock (ReadBuffer)
            {
                byte messageType = ReadBuffer[start];
                msgType = messageType;

                int realBalls = start + 1;
                MessageReader.BaseStream.Position = realBalls;

                BinaryReader reader = MessageReader;
                switch (messageType)
                {
                    case MessageID.PlayerInfo:
                    {
                        int playerIndex = MessageReader.ReadByte();
                        bool ServerWantsToRunCheckBytesInClientLoopThread = MessageReader.ReadBoolean();

                        if (Settings.PrintAnyOutput && Settings.PrintPlayerId)
                        {
                            Console.WriteLine($"Player id of {playerIndex}");
                        }

                        myPlayer = playerIndex;
                        LocalPlayer.active = true;
                        LocalPlayer.SyncDataWithTemp(PlayerFile);

                        SendData(MessageID.SyncPlayer, playerIndex);
                        SendData(MessageID.ClientUUID, playerIndex);
                        SendData(MessageID.PlayerLife, playerIndex);
                        SendData(MessageID.PlayerMana, playerIndex);
                        SendData(MessageID.SyncPlayerBuffs, playerIndex);

                        for (int i = 0; i < 260; i++)
                        {
                            SendData(MessageID.SyncEquipment, playerIndex, i, 0, 0, 0);
                        }

                        if (Settings.PrintAnyOutput && Settings.PrintWorldJoinMessages)
                        {
                            Console.WriteLine("Requesting world data");
                        }
                        SendData(MessageID.RequestWorldData);
                        break;
                    }
                    case MessageID.NetModules:
                    {
                        ushort num = MessageReader.ReadUInt16();

                        switch (num)
                        {
                            case 0:
                            {
                                int num420420 = MessageReader.ReadUInt16();
                                for (int i = 0; i < num420420; i++)
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
                            case 1:
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
                        CurrentWorld = new World();
                        CurrentWorld.time = reader.ReadInt32();
                        BitsByte bitsByte20 = reader.ReadByte();
                        CurrentWorld.dayTime = bitsByte20[0];
                        CurrentWorld.bloodMoon = bitsByte20[1];
                        CurrentWorld.eclipse = bitsByte20[2];
                        CurrentWorld.moonPhase = reader.ReadByte();
                        CurrentWorld.maxTilesX = reader.ReadInt16();
                        CurrentWorld.maxTilesY = reader.ReadInt16();
                        CurrentWorld.spawnTileX = reader.ReadInt16();
                        CurrentWorld.spawnTileY = reader.ReadInt16();
                        CurrentWorld.worldSurface = reader.ReadInt16();
                        CurrentWorld.rockLayer = reader.ReadInt16();
                        CurrentWorld.worldID = reader.ReadInt32();
                        CurrentWorld.worldName = reader.ReadString();
                        CurrentWorld.GameMode = reader.ReadByte();
                        CurrentWorld.worldUUID = new Guid(reader.ReadBytes(16));
                        CurrentWorld.worldGenVer = reader.ReadUInt64();
                        CurrentWorld.moonType = reader.ReadByte();

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

                        CurrentWorld.iceBackStyle = reader.ReadByte();
                        CurrentWorld.jungleBackStyle = reader.ReadByte();
                        CurrentWorld.hellBackStyle = reader.ReadByte();
                        CurrentWorld.windSpeedTarget = reader.ReadSingle();
                        CurrentWorld.numClouds = reader.ReadByte();

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

                        CurrentWorld.maxRaining = reader.ReadSingle();
                        CurrentWorld.raining = CurrentWorld.maxRaining > 0f;

                        BitsByte bitsByte21 = reader.ReadByte();
                        CurrentWorld.shadowOrbSmashed = bitsByte21[0];
                        CurrentWorld.downedBoss1 = bitsByte21[1];
                        CurrentWorld.downedBoss2 = bitsByte21[2];
                        CurrentWorld.downedBoss3 = bitsByte21[3];
                        CurrentWorld.hardMode = bitsByte21[4];
                        CurrentWorld.downedClown = bitsByte21[5];
                        ServerSideCharacter = bitsByte21[6];
                        CurrentWorld.downedPlantBoss = bitsByte21[7];
                        //if (Main.ServerSideCharacter)
                        //{
                        //    Main.ActivePlayerFileData.MarkAsServerSide();
                        //}
                        BitsByte bitsByte22 = reader.ReadByte();
                        CurrentWorld.downedMechBoss1 = bitsByte22[0];
                        CurrentWorld.downedMechBoss2 = bitsByte22[1];
                        CurrentWorld.downedMechBoss3 = bitsByte22[2];
                        CurrentWorld.downedMechBossAny = bitsByte22[3];
                        CurrentWorld.cloudBGActive = (bitsByte22[4] ? 1 : 0);
                        CurrentWorld.crimson = bitsByte22[5];
                        CurrentWorld.pumpkinMoon = bitsByte22[6];
                        CurrentWorld.snowMoon = bitsByte22[7];
                        BitsByte bitsByte23 = reader.ReadByte();
                        CurrentWorld.fastForwardTime = bitsByte23[1];
                        //UpdateTimeRate();
                        bool num265 = bitsByte23[2];
                        CurrentWorld.downedSlimeKing = bitsByte23[3];
                        CurrentWorld.downedQueenBee = bitsByte23[4];
                        CurrentWorld.downedFishron = bitsByte23[5];
                        CurrentWorld.downedMartians = bitsByte23[6];
                        CurrentWorld.downedAncientCultist = bitsByte23[7];
                        BitsByte bitsByte24 = reader.ReadByte();
                        CurrentWorld.downedMoonlord = bitsByte24[0];
                        CurrentWorld.downedHalloweenKing = bitsByte24[1];
                        CurrentWorld.downedHalloweenTree = bitsByte24[2];
                        CurrentWorld.downedChristmasIceQueen = bitsByte24[3];
                        CurrentWorld.downedChristmasSantank = bitsByte24[4];
                        CurrentWorld.downedChristmasTree = bitsByte24[5];
                        CurrentWorld.downedGolemBoss = bitsByte24[6];
                        CurrentWorld.BirthdayPartyManualParty = bitsByte24[7];
                        BitsByte bitsByte25 = reader.ReadByte();
                        CurrentWorld.downedPirates = bitsByte25[0];
                        CurrentWorld.downedFrost = bitsByte25[1];
                        CurrentWorld.downedGoblins = bitsByte25[2];
                        CurrentWorld.Sandstorm.Happening = bitsByte25[3];
                        CurrentWorld.DD2.Ongoing = bitsByte25[4];
                        CurrentWorld.DD2.DownedInvasionT1 = bitsByte25[5];
                        CurrentWorld.DD2.DownedInvasionT2 = bitsByte25[6];
                        CurrentWorld.DD2.DownedInvasionT3 = bitsByte25[7];
                        BitsByte bitsByte26 = reader.ReadByte();
                        CurrentWorld.combatBookWasUsed = bitsByte26[0];
                        CurrentWorld.LanternNightManualLanterns = bitsByte26[1];
                        CurrentWorld.downedTowerSolar = bitsByte26[2];
                        CurrentWorld.downedTowerVortex = bitsByte26[3];
                        CurrentWorld.downedTowerNebula = bitsByte26[4];
                        CurrentWorld.downedTowerStardust = bitsByte26[5];
                        CurrentWorld.forceHalloweenForToday = bitsByte26[6];
                        CurrentWorld.forceXMasForToday = bitsByte26[7];
                        BitsByte bitsByte27 = reader.ReadByte();
                        CurrentWorld.boughtCat = bitsByte27[0];
                        CurrentWorld.boughtDog = bitsByte27[1];
                        CurrentWorld.boughtBunny = bitsByte27[2];
                        CurrentWorld.freeCake = bitsByte27[3];
                        CurrentWorld.drunkWorld = bitsByte27[4];
                        CurrentWorld.downedEmpressOfLight = bitsByte27[5];
                        CurrentWorld.downedQueenSlime = bitsByte27[6];
                        CurrentWorld.getGoodWorld = bitsByte27[7];
                        BitsByte bitsByte28 = reader.ReadByte();
                        CurrentWorld.tenthAnniversaryWorld = bitsByte28[0];
                        CurrentWorld.dontStarveWorld = bitsByte28[1];
                        CurrentWorld.downedDeerclops = bitsByte28[2];
                        CurrentWorld.notTheBeesWorld = bitsByte28[3];
                        CurrentWorld.SavedOreTiers_Copper = reader.ReadInt16();
                        CurrentWorld.SavedOreTiers_Iron = reader.ReadInt16();
                        CurrentWorld.SavedOreTiers_Silver = reader.ReadInt16();
                        CurrentWorld.SavedOreTiers_Gold = reader.ReadInt16();
                        CurrentWorld.SavedOreTiers_Cobalt = reader.ReadInt16();
                        CurrentWorld.SavedOreTiers_Mythril = reader.ReadInt16();
                        CurrentWorld.SavedOreTiers_Adamantite = reader.ReadInt16();
                        if (num265)
                        {
                            //Main.StartSlimeRain();
                        }
                        else
                        {
                            //Main.StopSlimeRain();
                        }
                        CurrentWorld.invasionType = reader.ReadSByte();
                        LobbyId = reader.ReadUInt64();
                        CurrentWorld.Sandstorm.IntendedSeverity = reader.ReadSingle();

                        //CurrentWorld.tile = new Tile[CurrentWorld.maxTilesX,CurrentWorld.maxTilesY];

                        SendData(MessageID.SpawnTileData, CurrentWorld.spawnTileX, CurrentWorld.spawnTileY);

                        if (!joined)
                        {
                            joined = true;
                            if (Settings.PrintAnyOutput && Settings.PrintWorldJoinMessages)
                            {
                                Console.WriteLine($"Joining world \"{ CurrentWorld.worldName}\"");
                            }
                            if (Settings.SpawnPlayer)
                            {
                                SendData(MessageID.PlayerSpawn, myPlayer, 1);
                            }
                            WorldDataRecieved?.Invoke(this);
                        }
                        break;
                    }
                    case MessageID.FinishedConnectingToServer:
                    {
                        JoinedWorld?.Invoke(this);
                        break;
                    }
                    case MessageID.StatusText:
                    {
                        int statusMax = reader.ReadInt32();
                        string statusText = reader.ReadString();
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
                        CurrentWorld.DecompressTileSection(ReadBuffer, start, length);
                        break;
                    }
                    case MessageID.TileManipulation:
                    {
                        byte action = reader.ReadByte();
                        int tileX = reader.ReadInt16();
                        int tileY = reader.ReadInt16();
                        int flags = reader.ReadInt16();
                        int flags2 = reader.ReadByte();
                        break;
                    }
                    case MessageID.SyncPlayer:
                    {
                        byte whoAreThey = reader.ReadByte();


                        // skin variant
                        player[whoAreThey].skinVariant = reader.ReadByte();

                        // hair
                        reader.ReadByte();

                        player[whoAreThey].name = reader.ReadString();

                        // hair dye
                        player[whoAreThey].hairDye = reader.ReadByte();

                        // accessory/armor visibility 1
                        BitsByte hideVisibleAccessory = reader.ReadByte();

                        // accessory/armor visibility 2
                        BitsByte hideVisibleAccessory2 = reader.ReadByte();

                        // hide misc
                        reader.ReadByte();

                        // hairColor
                        player[whoAreThey].hairColor = reader.ReadRGB();

                        // skinColor
                        player[whoAreThey].skinColor = reader.ReadRGB();

                        // eyeColor
                        player[whoAreThey].eyeColor = reader.ReadRGB();

                        // shirtColor
                        player[whoAreThey].shirtColor = reader.ReadRGB();

                        // underShirtColor
                        player[whoAreThey].underShirtColor = reader.ReadRGB();

                        // pantsColor
                        player[whoAreThey].pantsColor = reader.ReadRGB();

                        // shoeColor
                        player[whoAreThey].shoeColor = reader.ReadRGB();

                        BitsByte bitsByte7 = reader.ReadByte();

                        BitsByte bitsByte8 = reader.ReadByte();

                        break;
                    }
                    case MessageID.PlayerActive:
                    {
                        byte whoAreThey = reader.ReadByte();
                        bool active = reader.ReadByte() == 1;

                        player[whoAreThey].active = active;
                        break;
                    }
                    case MessageID.PlayerControls:
                    {
                        byte whoAreThey = reader.ReadByte();

                        Player plr = player[whoAreThey];
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
                        Player plr = player[whoAreThey];

                        plr.statLife = reader.ReadInt16();
                        plr.statLifeMax = reader.ReadInt16();
                        break;
                    }
                    case MessageID.PlayerMana:
                    {
                        byte whoAreThey = reader.ReadByte();
                        Player plr = player[whoAreThey];

                        plr.statMana = reader.ReadInt16();
                        plr.statManaMax = reader.ReadInt16();
                        break;
                    }
                    case MessageID.SyncNPC:
                    {
                        int npcIndex = reader.ReadInt16();
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
        }

        public void SendData(int msid, int number = 0, float number2 = 0, float number3 = 0, float number4 = 0, int number5 = 0)
        {
            lock (WriteBuffer)
            {
                BinaryWriter writer = MessageWriter;

                writer.Seek(2, SeekOrigin.Begin);

                writer.Write((byte)msid);
                switch (msid)
                {
                    case MessageID.Hello:
                        writer.Write("Terraria" + 248);
                        break;
                    case MessageID.SyncPlayer:
                    {
                        Player plr = player[number];
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
                        Player plr = player[number];
                        writer.Write((byte)number);
                        //statLife
                        writer.Write((short)plr.statLife);
                        //statLifeMax
                        writer.Write((short)plr.statLifeMax);
                        break;
                    }
                    case MessageID.PlayerMana:
                    {
                        Player plr = player[number];
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
                        writer.Write((byte)number);
                        writer.Write((short)number2);

                        // type?
                        writer.Write((short)number3);
                        // prefix?
                        writer.Write((byte)number4);
                        // stack?
                        writer.Write((short)number5);
                    }
                    break;
                    case MessageID.SpawnTileData:
                        writer.Write((int)number);
                        writer.Write((int)number2);
                        break;
                    case MessageID.PlayerSpawn:
                    {
                        writer.Write((byte)number);
                        writer.Write((short)CurrentWorld.spawnTileX);
                        writer.Write((short)CurrentWorld.spawnTileY);
                        writer.Write(0);
                        writer.Write((byte)number2);
                        break;
                    }
                    case MessageID.PlayerControls:
                    {
                        Player plr = player[number];
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
                }

                int length = (int)MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);

                TCPClient.Send(WriteBuffer, length);
            }
        }

        public void SendChatMessage(string msg)
        {
            lock (WriteBuffer)
            {
                BinaryWriter writer = MessageWriter;

                writer.Seek(2, SeekOrigin.Begin);

                writer.Write((byte)MessageID.NetModules);

                // module type
                writer.Write((ushort)1);
                writer.Write((ushort)1);
                writer.Write(msg);

                int length = (int)MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);

                TCPClient.Send(WriteBuffer, length);
            }
        }

        public int FindPlayerByName(string name)
        {
            for (int i = 0; i < 255; i++)
            {
                if (player[i].active && player[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}