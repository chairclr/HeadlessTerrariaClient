using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Client;

namespace HeadlessTerrariaClient.Terraria
{
	public class World
    {

		/// <summary>
		/// A reference back to the owner ClientWorld, for access to things like Players[]
		/// </summary>
		public ClientWorld ClientWorld;

		#region World data
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

		public Tile[,] Tiles;
		public bool[,] LoadedTileSections;
		public int MaxTileSectionsX => maxTilesX / 200;
		public int MaxTileSectionsY => maxTilesY / 150;

		public Chest[] Chests = new Chest[8000];

		public Sign[] Signs = new Sign[1000];
		#endregion

		public int UnderworldLayer => maxTilesY - 200;

		public void SetupTiles(bool loadTileSections)
		{
			if (loadTileSections)
			{
				Tiles = new Tile[maxTilesX, maxTilesY];
			}

			LoadedTileSections = new bool[maxTilesX / 200, maxTilesY / 150];
		}
		
		/// <returns>Whether or not a tile section is loaded</returns>
		public bool IsTileSectionLoaded(int tileSectionX, int tileSectionY)
		{
			return LoadedTileSections[tileSectionX, tileSectionY];
		}

		/// <returns>Whether or not a tile is in a loaded tile section</returns>
		public bool IsTileInLoadedSection(int tileX, int tileY)
		{
			return IsTileSectionLoaded(tileX / 200, tileY / 150);
		}

        #region Decoding Tile Section
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
			LoadTileChunk_Tiles(reader, xStart, yStart, width, height);
			LoadTileChunk_Chests(reader, xStart, yStart, width, height);
			LoadTileChunk_Signs(reader, xStart, yStart, width, height);
			LoadTileChunk_TileEntities(reader, xStart, yStart, width, height);
		}

		public void LoadTileChunk_Tiles(BinaryReader reader, int xStart, int yStart, int width, int height)
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
						if (Tiles[j, i] == null)
						{
							Tiles[j, i] = new Tile(tileCache);
						}
						else
						{
							Tiles[j, i].CopyFrom(tileCache);
						}
						continue;
					}
					byte b;
					byte b2 = (b = 0);
					tileCache = Tiles[j, i];
					if (tileCache == null)
					{
						tileCache = new Tile();
						Tiles[j, i] = tileCache;
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
		}
		public void LoadTileChunk_Chests(BinaryReader reader, int xStart, int yStart, int width, int height)
		{
			short chestCount = reader.ReadInt16();
			for (int k = 0; k < chestCount; k++)
			{
				short chestId = reader.ReadInt16();
				short x = reader.ReadInt16();
				short y = reader.ReadInt16();
				string name = reader.ReadString();
				if (chestId >= 0 && chestId < 8000)
				{
					// add chests now
					if (Chests[chestId] == null)
					{
						Chests[chestId] = new Chest();
					}
					Chests[chestId].Name = name;
					Chests[chestId].x = x;
					Chests[chestId].y = y;
				}
			}
		}
		public void LoadTileChunk_Signs(BinaryReader reader, int xStart, int yStart, int width, int height)
		{
			short signCount = reader.ReadInt16();
			for (int l = 0; l < signCount; l++)
			{
				short signId = reader.ReadInt16();
				short x = reader.ReadInt16();
				short y = reader.ReadInt16();
				string text = reader.ReadString();
				if (signId >= 0 && signId < 1000)
				{
					// add signs now
					if (Signs[signId] == null)
					{
						Signs[signId] = new Sign();
					}
					Signs[signId].text = text;
					Signs[signId].x = x;
					Signs[signId].y = y;
				}
			}
		}
		public void LoadTileChunk_TileEntities(BinaryReader reader, int xStart, int yStart, int width, int height)
		{
			short tileEntityCount = reader.ReadInt16();
			for (int m = 0; m < tileEntityCount; m++)
			{
				// add tile entities later
				//TileEntity tileEntity = TileEntity.Read(reader);
				//TileEntity.ByID[tileEntity.ID] = tileEntity;
				//TileEntity.ByPosition[tileEntity.Position] = tileEntity;
			}
		}
        #endregion

        #region Bad Terraria code
        public bool CanPoundTile(int x, int y)
		{
			if (Tiles[x, y] == null)
			{
				Tiles[x, y] = new Tile();
			}
			if (Tiles[x, y - 1] == null)
			{
				Tiles[x, y - 1] = new Tile();
			}
			if (Tiles[x, y + 1] == null)
			{
				Tiles[x, y + 1] = new Tile();
			}
			switch (Tiles[x, y].tileType)
			{
				case 10:
				case 48:
				case 137:
				case 138:
				case 232:
				case 380:
				case 387:
				case 388:
				case 476:
				case 484:
					return false;
				default:
					if (Tiles[x, y - 1].GetTileActive())
					{
						switch (Tiles[x, y - 1].tileType)
						{
							case 21:
							case 26:
							case 77:
							case 88:
							case 235:
							case 237:
							case 441:
							case 467:
							case 468:
							case 470:
							case 475:
							case 488:
							case 597:
								return false;
						}
					}
					return true;
			}
		}
		public int CheckTileBreakability(int x, int y)
		{
			Tile tile = Tiles[x, y];
			if (y >= 1 && y <= maxTilesY - 1)
			{
				Tile tile2 = Tiles[x, y - 1];
				Tile tile3 = Tiles[x, y + 1];
				if (tile3 != null && tile3.GetTileActive())
				{
					return 2;
				}
				if (!TileID.IsTileSolid[tile.tileType] && !TileID.IsTileSolidTop[tile.tileType])
				{
					return 0;
				}
				if (tile2.GetTileActive())
				{
					if ((tile.tileType != tile2.tileType) | (tile2.tileType == 77 && tile.tileType != 77))
					{
						if (TileID.IsATreeTrunk[tile2.tileType])
						{
							return 2;
						}
						if (tile2.tileType == 323)
						{
							return 0;
						}
						return 2;
					}
					if (tile2.tileType == 80 && tile2.tileType != tile.tileType)
					{
						return 2;
					}
					if (tile.tileType == 10)
					{
						return 1;
					}
					if (tile.tileType == 138 || tile.tileType == 484)
					{
						return 0;
					}
				}
			}
			return 0;
		}
		public void KillTile(int i, int j, bool fail = false, bool noItem = true)
		{
			if (i < 0 || j < 0 || i >= maxTilesX || j >= maxTilesY)
			{
				return;
			}
			Tile tile = Tiles[i, j];
			if (tile == null)
			{
				tile = new Tile();
				Tiles[i, j] = tile;
			}
			if (!tile.GetTileActive())
			{
				return;
			}
			if (j >= 1 && Tiles[i, j - 1] == null)
			{
				Tiles[i, j - 1] = new Tile();
			}
			int num = CheckTileBreakability(i, j);
			if (num == 1)
			{
				fail = true;
			}
			if (num == 2)
			{
				return;
			}
			if (fail)
			{
				if (tile.tileType == 2 || tile.tileType == 23 || tile.tileType == 109 || tile.tileType == 199 || tile.tileType == 477 || tile.tileType == 492)
				{
					tile.tileType = 0;
				}
				if (tile.tileType == 60 || tile.tileType == 70)
				{
					tile.tileType = 59;
				}
				if (TileID.tileMoss[tile.tileType])
				{
					tile.tileType = 1;
				}
				if (TileID.tileMossBrick[tile.tileType])
				{
					tile.tileType = 38;
				}
				return;
			}
			
			if (tile.tileType == 51 && tile.wallType == 62 && Utility.Util.rand.Next(4) != 0)
			{
				noItem = true;
			}
			tile.SetTileActive(active: false);
			tile.SetHalfBrick(halfBrick: false);
			tile.SetTilePaint(0);
			if (tile.tileType == 58 && j > UnderworldLayer)
			{
				tile.SetIsLava(lava: true);
				tile.liquidCount = 128;
			}
			tile.tileType = 0;
			tile.SetInactive(inActive: false);
		}

		public void PlaceChestDirect(int x, int y, ushort type, int style, int id)
		{
			CreateChest(x, y - 1, id);
			for (int i = 0; i <= 1; i++)
			{
				for (int j = -1; j <= 0; j++)
				{
					if (Tiles[x + i, y + j] == null)
					{	
						Tiles[x + i, y + j] = new Tile();
					}
				}
			}
			Tiles[x, y - 1].SetTileActive(active: true);
			Tiles[x, y - 1].tileType = type;
			Tiles[x, y - 1].SetHalfBrick(halfBrick: false);
			Tiles[x + 1, y - 1].SetTileActive(active: true);
			Tiles[x + 1, y - 1].tileType = type;
			Tiles[x + 1, y - 1].SetHalfBrick(halfBrick: false);
			Tiles[x, y].SetTileActive(active: true);
			Tiles[x, y].tileType = type;
			Tiles[x, y].SetHalfBrick(halfBrick: false);
			Tiles[x + 1, y].SetTileActive(active: true);
			Tiles[x + 1, y].tileType = type;
			Tiles[x + 1, y].SetHalfBrick(halfBrick: false);
		}

		public void PlaceDresserDirect(int x, int y, ushort type, int style, int id)
		{
			CreateChest(x - 1, y - 1, id);
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 0; j++)
				{
					if (Tiles[x + i, y + j] == null)
					{
						Tiles[x + i, y + j] = new Tile();
					}
				}
			}
			short num = (short)(style * 54);
			Tiles[x - 1, y - 1].SetTileActive(active: true);
			Tiles[x - 1, y - 1].tileType = type;
			Tiles[x, y - 1].SetTileActive(active: true);
			Tiles[x, y - 1].tileType = type;
			Tiles[x + 1, y - 1].SetTileActive(active: true);
			Tiles[x + 1, y - 1].tileType = type;
			Tiles[x - 1, y].SetTileActive(active: true);
			Tiles[x - 1, y].tileType = type;
			Tiles[x, y].SetTileActive(active: true);
			Tiles[x, y].tileType = type;
			Tiles[x + 1, y].SetTileActive(active: true);
			Tiles[x + 1, y].tileType = type;
		}

		public int FindEmptyChest(int x, int y, int type = 21, int style = 0, int direction = 1, int alternate = 0)
		{
			int num = -1;
			for (int i = 0; i < 8000; i++)
			{
				Chest chest = Chests[i];
				if (chest != null)
				{
					if (chest.x == x && chest.y == y)
					{
						return -1;
					}
				}
				else if (num == -1)
				{
					num = i;
				}
			}
			return num;
		}

		public int CreateChest(int x, int y, int id = -1)
        {
			int num = id;
			if (num == -1)
			{
				num = FindEmptyChest(x, y);
				if (num == -1)
				{
					return -1;
				}
				return num;
			}
			Chests[num] = new Chest();
			Chests[num].x = x;
			Chests[num].y = y;
			for (int i = 0; i < 40; i++)
			{
				Chests[num].Items[i] = new Item();
			}
			return num;
		}

		public void KillChestDirect(int x, int y, int id)
        {
			if (id < 0 || id >= Chests.Length)
			{
				return;
			}
			try
			{
				Chest chest = Chests[id];
				if (chest != null && chest.x == x && chest.y == y)
				{
					Chests[id] = null;
				}
			}
			catch
			{
			}
		}
		#endregion
	}
}
