using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using System.Text;

namespace Terraria
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

        public void DecompressTileSection(byte[] buffer, int start, int length)
        {

        }
    }
}
