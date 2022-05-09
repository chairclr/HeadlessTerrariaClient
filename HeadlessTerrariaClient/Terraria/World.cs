using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace HeadlessTerrariaClient.Terraria
{
    public class World
    {
        public int time;
        public bool dayTime;
        public bool bloodMoon;
        public bool eclipse;
        public int moonPhase;
        public int maxTilesX;
        public int maxTilesY;
        public int spawnTileX;
        public int spawnTileY;
        public int worldSurface;
        public int rockLayer;
        public int worldID;
        public string worldName;
        public int GameMode;
        public Guid worldUUID;
        public ulong worldGenVer;
        public int moonType;
        public int iceBackStyle;
        public int jungleBackStyle;
        public int hellBackStyle;
        public float windSpeedTarget;
        public int numClouds;
        public float maxRaining;
        public bool raining;
        public bool shadowOrbSmashed;
        public bool downedBoss1;
        public bool downedBoss2;
        public bool downedBoss3;
        public bool hardMode;
        public bool downedClown;
        public bool downedPlantBoss;
        public bool downedMechBoss1;
        public bool downedMechBoss2;
        public bool downedMechBoss3;
        public bool downedMechBossAny;
        public bool crimson;
        public bool downedSlimeKing;
        public bool downedQueenBee;
        public bool downedFishron;
        public bool downedMartians;
        public bool downedAncientCultist;
        public bool downedMoonlord;
        public bool downedHalloweenKing;
        public bool downedHalloweenTree;
        public bool downedChristmasIceQueen;
        public bool downedChristmasSantank;
        public bool downedChristmasTree;
        public bool downedGolemBoss;
        public bool downedPirates;
        public bool downedFrost;
        public bool downedGoblins;
        public bool combatBookWasUsed;
        public bool downedTowerSolar;
        public bool downedTowerVortex;
        public bool downedTowerNebula;
        public bool downedTowerStardust;
        public bool boughtCat;
        public bool boughtDog;
        public bool boughtBunny;
        public bool freeCake;
        public bool downedEmpressOfLight;
        public bool downedQueenSlime;
        public bool downedDeerclops;
        public bool BirthdayPartyManualParty;
        public int cloudBGActive;
        public bool pumpkinMoon;
        public bool snowMoon;
        public bool fastForwardTime;
        public bool LanternNightManualLanterns;
        public bool forceHalloweenForToday;
        public bool forceXMasForToday;
        public bool drunkWorld;
        public bool getGoodWorld;
        public bool tenthAnniversaryWorld;
        public bool dontStarveWorld;
        public bool notTheBeesWorld;
        public int SavedOreTiers_Copper;
        public int SavedOreTiers_Iron;
        public int SavedOreTiers_Silver;
        public int SavedOreTiers_Gold;
        public int SavedOreTiers_Cobalt;
        public int SavedOreTiers_Mythril;
        public int SavedOreTiers_Adamantite;
        public sbyte invasionType;

        public SandstormInfo Sandstorm = new SandstormInfo();
        public DD2Info DD2 = new DD2Info();

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

        public Tile[,] tile;
        public bool[,] LoadedTileSections;

        public void SetupTiles(bool loadTileSections)
        {
            if (loadTileSections)
            {
                tile = new Tile[maxTilesX, maxTilesY];
            }

            LoadedTileSections = new bool[maxTilesX / 200, maxTilesY / 150];
        }

        public bool IsTileSectionLoaded(int tileSectionX, int tileSectionY)
        {
            return LoadedTileSections[tileSectionX, tileSectionY];
        }

        public bool IsTileInLoadedSection(int tileX, int tileY)
        {
            return IsTileSectionLoaded(tileX / 200, tileY / 150);
        }

        public void DecompressTileSection(byte[] buffer, int bufferStart, int bufferLength, bool loadTileSections)
        {
            // implement now ok
            using MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(buffer, bufferStart, bufferLength);
            memoryStream.Position = 0L;
            MemoryStream memoryStream3;
            if (memoryStream.ReadByte() != 0)
            {
                MemoryStream memoryStream2 = new MemoryStream();
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    deflateStream.CopyTo(memoryStream2);
                    deflateStream.Close();
                }
                memoryStream3 = memoryStream2;
                memoryStream3.Position = 0L;
            }
            else
            {
                memoryStream3 = memoryStream;
                memoryStream3.Position = 1L;
            }
            using BinaryReader binaryReader = new BinaryReader(memoryStream3);
            int xStart = binaryReader.ReadInt32();
            int yStart = binaryReader.ReadInt32();
            short width = binaryReader.ReadInt16();
            short height = binaryReader.ReadInt16();

            if (xStart % 200 == 0 && yStart % 150 == 0 && width == 200 && height == 150)
            {
                if (!IsTileInLoadedSection(xStart, yStart))
                {
                    LoadedTileSections[xStart / 200, yStart / 150] = true;

                    if (loadTileSections)
                    {
                        // implement actually loading tiles later on in the future ok
                    }
                }
                else
                {
                    // Already loaded this tile section on another client or something
                }
            }
        }
    }
}
