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

    public GameMode Mode;

    public int WorldSurface;

    public int RockLayer;

    public int UnderworldLayer => Height - 200;

    public int Time;

    public bool DayTime;

    public bool BloodMoon;

    public bool Eclipse;

    public int MoonType;

    public int IceBackStyle;

    public int JungleBackStyle;

    public int HellBackStyle;

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

    public bool DownedSlimeKing;

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

    public bool FastForwardTime;

    public bool LanternNightManualLanterns;

    public bool ForceHalloweenForToday;

    public bool ForceXMasForToday;

    public bool DrunkWorld;

    public bool GetGoodWorld;

    public bool TenthAnniversaryWorld;

    public bool DontStarveWorld;

    public bool NotTheBeesWorld;

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

    public Guid WorldUUID;

    public ulong WorldGenVer;

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
}
