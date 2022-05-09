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
            using MemoryStream rawSectionStream = new MemoryStream();
            rawSectionStream.Write(buffer, bufferStart, bufferLength);
            rawSectionStream.Position = 0L;
            MemoryStream tileSectionStream;
            if (rawSectionStream.ReadByte() != 0)
            {
                MemoryStream decompressedSectionStream = new MemoryStream();
                using (DeflateStream deflateStream = new DeflateStream(rawSectionStream, CompressionMode.Decompress, true))
                {
                    deflateStream.CopyTo(decompressedSectionStream);
                    deflateStream.Close();
                }
                tileSectionStream = decompressedSectionStream;
                tileSectionStream.Position = 0L;
            }
            else
            {
                tileSectionStream = rawSectionStream;
                tileSectionStream.Position = 1L;
            }
            using BinaryReader tileSectionReader = new BinaryReader(tileSectionStream);
            int xStart = tileSectionReader.ReadInt32();
            int yStart = tileSectionReader.ReadInt32();
            short width = tileSectionReader.ReadInt16();
            short height = tileSectionReader.ReadInt16();

            if (xStart % 200 == 0 && yStart % 150 == 0 && width == 200 && height == 150)
            {
                if (!IsTileInLoadedSection(xStart, yStart))
                {
                    LoadedTileSections[xStart / 200, yStart / 150] = true;

                    if (loadTileSections)
                    {
						LoadTileChunk(tileSectionReader, xStart, yStart, width, height);
                    }
                }
                else
                {
                    // Already loaded this tile section on another client or something
                }
            }
        }

		public void LoadTileChunk(BinaryReader reader, int xStart, int yStart, int width, int height)
		{
			Tile tileCache = null;
			int num = 0;
			for (int i = yStart; i < yStart + height; i++)
			{
				for (int j = xStart; j < xStart + width; j++)
				{
					if (num != 0)
					{
						num--;
						if (tile[j, i] == null)
						{
							tile[j, i] = new Tile(tileCache);
						}
						else
						{
							tile[j, i].CopyFrom(tileCache);
						}
						continue;
					}
					byte b;
					byte b2 = (b = 0);
					tileCache = tile[j, i];
					if (tileCache == null)
					{
						tileCache = new Tile();
						tile[j, i] = tileCache;
					}
					else
					{
						tileCache.ClearEverything();
					}
					byte b3 = reader.ReadByte();
					if ((b3 & 1) == 1)
					{
						b2 = reader.ReadByte();
						if ((b2 & 1) == 1)
						{
							b = reader.ReadByte();
						}
					}
					bool flag = tileCache.GetTileActive();
					byte b4;
					if ((b3 & 2) == 2)
					{
						tileCache.SetTileActive(active: true);
						ushort type = tileCache.tileType;
						int num2;
						if ((b3 & 0x20) == 32)
						{
							b4 = reader.ReadByte();
							num2 = reader.ReadByte();
							num2 = (num2 << 8) | b4;
						}
						else
						{
							num2 = reader.ReadByte();
						}
						tileCache.tileType = (ushort)num2;
						if (Terraria.ID.TileID.IsTileFrameImportant[num2])
						{
							int frameX = reader.ReadInt16();
							int frameY = reader.ReadInt16();
						}
						if ((b & 8) == 8)
						{
							tileCache.SetTilePaint(reader.ReadByte());
						}
					}
					if ((b3 & 4) == 4)
					{
						tileCache.wallType = reader.ReadByte();
						if ((b & 0x10) == 16)
						{
							tileCache.SetWallPaint(reader.ReadByte());
						}
					}
					b4 = (byte)((b3 & 0x18) >> 3);
					if (b4 != 0)
					{
						tileCache.liquidCount = reader.ReadByte();
						if (b4 > 1)
						{
							if (b4 == 2)
							{
								tileCache.SetIsLava(lava: true);
							}
							else
							{
								tileCache.SetIsHoney(honey: true);
							}
						}
					}
					if (b2 > 1)
					{
						if ((b2 & 2) == 2)
						{
							tileCache.SetWire(wire: true);
						}
						if ((b2 & 4) == 4)
						{
							tileCache.SetWire2(wire2: true);
						}
						if ((b2 & 8) == 8)
						{
							tileCache.SetWire3(wire3: true);
						}
						b4 = (byte)((b2 & 0x70) >> 4);
						if (b4 != 0)
						{
							if (b4 == 1)
							{
								tileCache.SetHalfBrick(halfBrick: true);
							}
							else
							{
								tileCache.SetSlopeType((byte)(b4 - 1));
							}
						}
					}
					if (b > 0)
					{
						if ((b & 2) == 2)
						{
							tileCache.SetActuator(actuator: true);
						}
						if ((b & 4) == 4)
						{
							tileCache.SetInactive(inActive: true);
						}
						if ((b & 0x20) == 32)
						{
							tileCache.SetWire4(wire4: true);
						}
						if ((b & 0x40) == 64)
						{
							b4 = reader.ReadByte();
							tileCache.wallType = (ushort)((b4 << 8) | tileCache.wallType);
						}
					}
					num = (byte)((b3 & 0xC0) >> 6) switch
					{
						0 => 0,
						1 => reader.ReadByte(),
						_ => reader.ReadInt16(),
					};
				}
			}
			short chestCount = reader.ReadInt16();
			for (int k = 0; k < chestCount; k++)
			{
				short chestId = reader.ReadInt16();
				short x = reader.ReadInt16();
				short y = reader.ReadInt16();
				string name = reader.ReadString();
				if (chestId >= 0 && chestId < 8000)
				{
					// add chests later
					//if (Main.chest[num4] == null)
					//{
					//	Main.chest[num4] = new Chest();
					//}
					//Main.chest[num4].name = name;
					//Main.chest[num4].x = x;
					//Main.chest[num4].y = y;
				}
			}
			short signCount = reader.ReadInt16();
			for (int l = 0; l < signCount; l++)
			{
				short signId = reader.ReadInt16();
				short x = reader.ReadInt16();
				short y = reader.ReadInt16();
				string text = reader.ReadString();
				if (signId >= 0 && signId < 1000)
				{
					// add signs later
					//if (Main.sign[num5] == null)
					//{
					//	Main.sign[num5] = new Sign();
					//}
					//Main.sign[num5].text = text;
					//Main.sign[num5].x = x;
					//Main.sign[num5].y = y;
				}
			}
			short tileEntityCount = reader.ReadInt16();
			for (int m = 0; m < tileEntityCount; m++)
			{
				// add tile entities later
				//TileEntity tileEntity = TileEntity.Read(reader);
				//TileEntity.ByID[tileEntity.ID] = tileEntity;
				//TileEntity.ByPosition[tileEntity.Position] = tileEntity;
			}
		}
	}
}
