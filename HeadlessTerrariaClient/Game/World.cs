using System.Runtime.CompilerServices;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient.Game;

public class World
{
    public Player[] Players = new Player[256];

    public string Name = "<unnamed world>";

    public NativeTileMap Tile = new NativeTileMap(8401, 2401);

    public int Width;

    public int Height;

    public int SpawnTileX;

    public int SpawnTileY;

    public GameMode GameMode;

    public bool ServerSideCharacters;

    public int WorldSurface;

    public int RockLayer;

    public int UnderworldLayer => Height - 200;

    public int Time;

    public bool DayTime;

    public bool BloodMoon;

    public bool Eclipse;

    public int MoonType;

    public int MoonPhase;

    public int IceBackgroundStyle;

    public int JungleBackgroundStyle;

    public int HellBackgroundStyle;

    public float WindSpeedTarget;

    public int NumClouds;

    public float MaxRaining;

    public bool Raining;

    public bool ShadowOrbSmashed;

    public bool DownedBoss1;

    public bool DownedBoss2;

    public bool DownedBoss3;

    public bool HardMode;

    public bool DownedClown;

    public bool DownedPlantBoss;

    public bool DownedMechBoss1;

    public bool DownedMechBoss2;

    public bool DownedMechBoss3;

    public bool DownedMechBossAny;

    public bool Crimson;

    public bool DownedKingSlime;

    public bool DownedQueenBee;

    public bool DownedFishron;

    public bool DownedMartians;

    public bool DownedAncientCultist;

    public bool DownedMoonlord;

    public bool DownedHalloweenKing;

    public bool DownedHalloweenTree;

    public bool DownedChristmasIceQueen;

    public bool DownedChristmasSantank;

    public bool DownedChristmasTree;

    public bool DownedGolemBoss;

    public bool DownedPirates;

    public bool DownedFrost;

    public bool DownedGoblins;

    public bool CombatBookWasUsed;

    public bool DownedTowerSolar;

    public bool DownedTowerVortex;

    public bool DownedTowerNebula;

    public bool DownedTowerStardust;

    public bool BoughtCat;

    public bool BoughtDog;

    public bool BoughtBunny;

    public bool FreeCake;

    public bool DownedEmpressOfLight;

    public bool DownedQueenSlime;

    public bool DownedDeerclops;

    public bool BirthdayPartyManualParty;

    public int CloudBGActive;

    public bool PumpkinMoon;

    public bool SnowMoon;

    public bool FastForwardTimeToDawn;

    public bool LanternNightManualLanterns;

    public bool ForceHalloweenForToday;

    public bool ForceXMasForToday;

    public bool DrunkWorld;

    public bool GetGoodWorld;

    public bool TenthAnniversaryWorld;

    public bool DontStarveWorld;

    public bool NotTheBeesWorld;

    public bool RemixWorld;

    public bool UnlockedSlimeBlueSpawn;

    public bool CombatBookVolumeTwoWasUsed;

    public bool PeddlersSatchelWasUsed;

    public int SavedOreTiers_Copper;

    public int SavedOreTiers_Iron;

    public int SavedOreTiers_Silver;

    public int SavedOreTiers_Gold;

    public int SavedOreTiers_Cobalt;

    public int SavedOreTiers_Mythril;

    public int SavedOreTiers_Adamantite;

    public sbyte InvasionType;

    public SandstormInfo Sandstorm = new SandstormInfo();

    public DD2Info DD2 = new DD2Info();

    public int ID;

    public ulong LobbyID;

    public Guid WorldUUID;

    public ulong WorldGenVer;

    public bool UnlockedSlimeGreenSpawn;

    public bool UnlockedSlimeOldSpawn;

    public bool UnlockedSlimePurpleSpawn;

    public bool UnlockedSlimeRainbowSpawn;

    public bool UnlockedSlimeRedSpawn;

    public bool UnlockedSlimeYellowSpawn;

    public bool UnlockedSlimeCopperSpawn;

    public bool FastForwardTimeToDusk;

    public bool NoTrapsWorld;

    public bool ZenithWorld;

    public bool UnlockedTruffleSpawn;

    public byte SundialCooldown;

    public byte MoondialCooldown;

    public class SandstormInfo
    {
        public bool Happening;
        public float IntendedSeverity;
    }

    public class DD2Info
    {
        public bool Ongoing;
        public bool DownedInvasionT1;
        public bool DownedInvasionT2;
        public bool DownedInvasionT3;
    }

    public World()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i] = new Player();
        }

        UpdatePlayerIndexes();
    }

    public void UpdatePlayerIndexes()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].Index = i;
        }
    }

    internal void HandleWorldData(BinaryReader reader)
    {
        Time = reader.ReadInt32();

        BitsByte timeEvent = reader.ReadByte();

        DayTime = timeEvent[0];
        BloodMoon = timeEvent[1];
        Eclipse = timeEvent[2];

        MoonPhase = reader.ReadByte();

        Width = reader.ReadInt16();
        Height = reader.ReadInt16();

        SpawnTileX = reader.ReadInt16();
        SpawnTileY = reader.ReadInt16();

        WorldSurface = reader.ReadInt16();
        RockLayer = reader.ReadInt16();

        ID = reader.ReadInt32();

        Name = reader.ReadString();

        GameMode = (GameMode)reader.ReadByte();

        WorldUUID = new Guid(reader.ReadBytes(16));

        WorldGenVer = reader.ReadUInt64();

        MoonType = reader.ReadByte();


        // WorldGen.setBG(0,  reader.ReadByte());
        // WorldGen.setBG(10, reader.ReadByte());
        // WorldGen.setBG(11, reader.ReadByte());
        // WorldGen.setBG(12, reader.ReadByte());
        // WorldGen.setBG(1,  reader.ReadByte());
        // WorldGen.setBG(2,  reader.ReadByte());
        // WorldGen.setBG(3,  reader.ReadByte());
        // WorldGen.setBG(4,  reader.ReadByte());
        // WorldGen.setBG(5,  reader.ReadByte());
        // WorldGen.setBG(6,  reader.ReadByte());
        // WorldGen.setBG(7,  reader.ReadByte());
        // WorldGen.setBG(8,  reader.ReadByte());
        // WorldGen.setBG(9,  reader.ReadByte());

        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();
        reader.ReadByte();

        IceBackgroundStyle = reader.ReadByte();

        JungleBackgroundStyle = reader.ReadByte();

        HellBackgroundStyle = reader.ReadByte();

        WindSpeedTarget = reader.ReadSingle();

        NumClouds = reader.ReadByte();

        for (int i = 0; i < 3; i++)
        {
            // Main.treeX[i]
            reader.ReadInt32();
        }

        for (int i = 0; i < 4; i++)
        {
            // Main.treeStyle[i]
            reader.ReadByte();
        }

        for (int i = 0; i < 3; i++)
        {
            // Main.caveBackX[i]
            reader.ReadInt32();
        }

        for (int i = 0; i < 4; i++)
        {
            // Main.caveBackStyle[i]
            reader.ReadByte();
        }

        for (int i = 0; i < 13; i++)
        {
            // Tree top variations[i]
            reader.ReadByte();
        }

        MaxRaining = reader.ReadSingle();

        BitsByte bossAndMiscInfo = reader.ReadByte();
        ShadowOrbSmashed = bossAndMiscInfo[0];
        DownedBoss1 = bossAndMiscInfo[1];
        DownedBoss2 = bossAndMiscInfo[2];
        DownedBoss3 = bossAndMiscInfo[3];
        HardMode = bossAndMiscInfo[4];
        DownedClown = bossAndMiscInfo[5];
        ServerSideCharacters = bossAndMiscInfo[6];
        DownedPlantBoss = bossAndMiscInfo[7];

        BitsByte bossAndMiscInfo2 = reader.ReadByte();
        FastForwardTimeToDawn = bossAndMiscInfo2[1];
        bool slimeRain = bossAndMiscInfo[2];
        DownedKingSlime = bossAndMiscInfo2[3];
        DownedQueenBee = bossAndMiscInfo2[4];
        DownedFishron = bossAndMiscInfo2[5];
        DownedMartians = bossAndMiscInfo2[6];
        DownedAncientCultist = bossAndMiscInfo2[7];

        BitsByte bossAndMiscInfo3 = reader.ReadByte();
        DownedMoonlord = bossAndMiscInfo3[0];
        DownedHalloweenKing = bossAndMiscInfo3[1];
        DownedHalloweenTree = bossAndMiscInfo3[2];
        DownedChristmasIceQueen = bossAndMiscInfo3[3];
        DownedChristmasSantank = bossAndMiscInfo3[4];
        DownedChristmasTree = bossAndMiscInfo3[5];
        DownedGolemBoss = bossAndMiscInfo3[6];
        BirthdayPartyManualParty = bossAndMiscInfo3[7];

        BitsByte bossAndMiscInfo4 = reader.ReadByte();
        DownedPirates = bossAndMiscInfo4[0];
        DownedFrost = bossAndMiscInfo4[1];
        DownedGoblins = bossAndMiscInfo4[2];
        Sandstorm.Happening = bossAndMiscInfo[3];
        DD2.Ongoing = bossAndMiscInfo[4];
        DD2.DownedInvasionT1 = bossAndMiscInfo[5];
        DD2.DownedInvasionT2 = bossAndMiscInfo[6];
        DD2.DownedInvasionT3 = bossAndMiscInfo[7];

        BitsByte npcAndWorldInfo = reader.ReadByte();
        BoughtCat = npcAndWorldInfo[0];
        BoughtDog = npcAndWorldInfo[1];
        BoughtBunny = npcAndWorldInfo[2];
        FreeCake = npcAndWorldInfo[3];
        DrunkWorld = npcAndWorldInfo[4];
        DownedEmpressOfLight = npcAndWorldInfo[5];
        DownedQueenSlime = npcAndWorldInfo[6];
        GetGoodWorld = npcAndWorldInfo[7];

        BitsByte npcAndWorldInfo2 = reader.ReadByte();
        TenthAnniversaryWorld = npcAndWorldInfo2[0];
        DontStarveWorld = npcAndWorldInfo2[1];
        DownedDeerclops = npcAndWorldInfo2[2];
        NotTheBeesWorld = npcAndWorldInfo2[3];
        RemixWorld = npcAndWorldInfo2[4];
        UnlockedSlimeBlueSpawn = npcAndWorldInfo2[5];
        CombatBookVolumeTwoWasUsed = npcAndWorldInfo2[6];
        PeddlersSatchelWasUsed = npcAndWorldInfo2[7];

        BitsByte npcAndWorldInfo3 = reader.ReadByte();
        UnlockedSlimeGreenSpawn = npcAndWorldInfo3[0];
        UnlockedSlimeOldSpawn = npcAndWorldInfo3[1];
        UnlockedSlimePurpleSpawn = npcAndWorldInfo3[2];
        UnlockedSlimeRainbowSpawn = npcAndWorldInfo3[3];
        UnlockedSlimeRedSpawn = npcAndWorldInfo3[4];
        UnlockedSlimeYellowSpawn = npcAndWorldInfo3[5];
        UnlockedSlimeCopperSpawn = npcAndWorldInfo3[6];
        FastForwardTimeToDusk = npcAndWorldInfo3[7];

        BitsByte npcAndWorldInfo4 = reader.ReadByte();
        NoTrapsWorld = npcAndWorldInfo4[0];
        ZenithWorld = npcAndWorldInfo4[1];
        UnlockedTruffleSpawn = npcAndWorldInfo4[2];

        SundialCooldown = reader.ReadByte();
        MoondialCooldown = reader.ReadByte();

        SavedOreTiers_Copper = reader.ReadInt16();
        SavedOreTiers_Iron = reader.ReadInt16();
        SavedOreTiers_Silver = reader.ReadInt16();
        SavedOreTiers_Gold = reader.ReadInt16();
        SavedOreTiers_Cobalt = reader.ReadInt16();
        SavedOreTiers_Mythril = reader.ReadInt16();
        SavedOreTiers_Adamantite = reader.ReadInt16();

        InvasionType = reader.ReadSByte();

        LobbyID = reader.ReadUInt64();

        Sandstorm.IntendedSeverity = reader.ReadSingle();
    }

    internal void HandleTileSection(BinaryReader reader, int xStart, int yStart, int width, int height)
    {
        HandleSectionTiles(reader, xStart, yStart, width, height);
    }

    private void HandleSectionTiles(BinaryReader reader, int xStart, int yStart, int width, int height)
    {
        ref TileData tile = ref Unsafe.NullRef<TileData>();
        int num = 0;
        for (int i = yStart; i < yStart + height; i++)
        {
            for (int j = xStart; j < xStart + width; j++)
            {
                if (num != 0)
                {
                    num--;
                    Tile.GetTileRef(j, i).CopyFrom(ref tile);
                    continue;
                }

                byte b2;
                byte b;
                byte b3 = (b2 = (b = 0));
                tile = ref Tile.GetTileRef(j, i);
                tile.ClearEverything();

                byte b4 = reader.ReadByte();
                bool flag = false;
                if ((b4 & 1) == 1)
                {
                    flag = true;
                    b3 = reader.ReadByte();
                }

                bool flag2 = false;
                if (flag && (b3 & 1) == 1)
                {
                    flag2 = true;
                    b2 = reader.ReadByte();
                }

                if (flag2 && (b2 & 1) == 1)
                {
                    b = reader.ReadByte();
                }

                bool flag3 = tile.Active;
                byte b5;
                if ((b4 & 2) == 2)
                {
                    tile.Active = true;
                    ushort type = tile.Type;
                    int num2;
                    if ((b4 & 0x20) == 32)
                    {
                        b5 = reader.ReadByte();
                        num2 = reader.ReadByte();
                        num2 = (num2 << 8) | b5;
                    }
                    else
                    {
                        num2 = reader.ReadByte();
                    }

                    tile.Type = (ushort)num2;
                    if (TileFrame.TileFrameImportant[num2])
                    {
                        tile.FrameX = reader.ReadInt16();
                        tile.FrameY = reader.ReadInt16();
                    }
                    else if (!flag3 || tile.Type != type)
                    {
                        tile.FrameX = -1;
                        tile.FrameY = -1;
                    }

                    if ((b2 & 8) == 8)
                    {
                        tile.Color = reader.ReadByte();
                    }
                }

                if ((b4 & 4) == 4)
                {
                    tile.Wall = reader.ReadByte();
                    if ((b2 & 0x10) == 16)
                    {
                        tile.WallColor = reader.ReadByte();
                    }
                }

                b5 = (byte)((b4 & 0x18) >> 3);
                if (b5 != 0)
                {
                    tile.LiquidAmount = reader.ReadByte();
                    if ((b2 & 0x80) == 128)
                    {
                        tile.Shimmer = true;
                    }
                    else if (b5 > 1)
                    {
                        if (b5 == 2)
                        {
                            tile.Lava = true;
                        }
                        else
                        {
                            tile.Honey = true;
                        }
                    }
                }

                if (b3 > 1)
                {
                    if ((b3 & 2) == 2)
                    {
                        tile.Wire = true;
                    }

                    if ((b3 & 4) == 4)
                    {
                        tile.Wire2 = true;
                    }

                    if ((b3 & 8) == 8)
                    {
                        tile.Wire3 = true;
                    }

                    b5 = (byte)((b3 & 0x70) >> 4);
                    if (b5 != 0/* && Main.tileSolid[tile.type]*/)
                    {
                        if (b5 == 1)
                        {
                            tile.HalfBrick = true;
                        }
                        else
                        {
                            tile.Slope = (byte)(b5 - 1);
                        }
                    }
                }

                if (b2 > 1)
                {
                    if ((b2 & 2) == 2)
                    {
                        tile.Actuator = true;
                    }

                    if ((b2 & 4) == 4)
                    {
                        tile.InActive = true;
                    }

                    if ((b2 & 0x20) == 32)
                    {
                        tile.Wire4 = true;
                    }

                    if ((b2 & 0x40) == 64)
                    {
                        b5 = reader.ReadByte();
                        tile.Wall = (ushort)((b5 << 8) | tile.Wall);
                    }
                }

                if (b > 1)
                {
                    if ((b & 2) == 2)
                    {
                        tile.InvisibleBlock = true;
                    }

                    if ((b & 4) == 4)
                    {
                        tile.InvisibleWall = true;
                    }

                    if ((b & 8) == 8)
                    {
                        tile.FullbrightBlock = true;
                    }

                    if ((b & 0x10) == 16)
                    {
                        tile.FullbrightWall = true;
                    }
                }

                num = (byte)((b4 & 0xC0) >> 6) switch
                {
                    0 => 0,
                    1 => reader.ReadByte(),
                    _ => reader.ReadInt16(),
                };
            }
        }
    }
}
