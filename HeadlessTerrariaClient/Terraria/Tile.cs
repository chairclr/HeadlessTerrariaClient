using System.Numerics;
using HeadlessTerrariaClient.Utility;

namespace HeadlessTerrariaClient.Terraria
{
    public class Tile
    {
		public ushort tileType;

		public ushort wallType;

		public byte liquidCount;

		public short shortTileHeader;

		public byte byteTileHeader;

		public byte byteTileHeader2;

		public byte byteTileHeader3;

		public Tile()
		{
			tileType = 0;
			wallType = 0;
			liquidCount = 0;
			shortTileHeader = 0;
			byteTileHeader = 0;
			byteTileHeader2 = 0;
			byteTileHeader3 = 0;
		}

		public Tile(Tile copy)
		{
			if (copy == null)
			{
				tileType = 0;
				wallType = 0;
				liquidCount = 0;
				shortTileHeader = 0;
				byteTileHeader = 0;
				byteTileHeader2 = 0;
				byteTileHeader3 = 0;
			}
			else
			{
				tileType = copy.tileType;
				wallType = copy.wallType;
				liquidCount = copy.liquidCount;
				shortTileHeader = copy.shortTileHeader;
				byteTileHeader = copy.byteTileHeader;
				byteTileHeader2 = copy.byteTileHeader2;
				byteTileHeader3 = copy.byteTileHeader3;
			}
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		public void ClearEverything()
		{
			tileType = 0;
			wallType = 0;
			liquidCount = 0;
			shortTileHeader = 0;
			byteTileHeader = 0;
			byteTileHeader2 = 0;
			byteTileHeader3 = 0;
		}

		public void ClearTile()
		{
			SetSlopeType(0);
			SetHalfBrick(halfBrick: false);
			SetTileActive(active: false);
			SetInactive(inActive: false);
		}

		public void CopyFrom(Tile from)
		{
			tileType = from.tileType;
			wallType = from.wallType;
			liquidCount = from.liquidCount;
			shortTileHeader = from.shortTileHeader;
			byteTileHeader = from.byteTileHeader;
			byteTileHeader2 = from.byteTileHeader2;
			byteTileHeader3 = from.byteTileHeader3;
		}

		public bool Equals(Tile compTile)
		{
			if (compTile == null)
			{
				return false;
			}
			if (shortTileHeader != compTile.shortTileHeader)
			{
				return false;
			}
			if (GetTileActive())
			{
				if (tileType != compTile.tileType)
				{
					return false;
				}
			}
			if (wallType != compTile.wallType || liquidCount != compTile.liquidCount)
			{
				return false;
			}
			if (compTile.liquidCount == 0)
			{
				if (GetWallPaint() != compTile.GetWallPaint())
				{
					return false;
				}
				if (GetWire4() != compTile.GetWire4())
				{
					return false;
				}
			}
			else if (byteTileHeader != compTile.byteTileHeader)
			{
				return false;
			}
			return true;
		}

		public int SlopeType()
		{
			if (GetHalfBrick())
			{
				return 1;
			}
			int num = GetSlopeType();
			if (num > 0)
			{
				num++;
			}
			return num;
		}

		public void SetLiquidType(int liquidType)
		{
			switch (liquidType)
			{
				case 0:
					byteTileHeader &= 159;
					break;
				case 1:
					SetIsLava(lava: true);
					break;
				case 2:
					SetIsHoney(honey: true);
					break;
			}
		}

		public byte GetLiquidType()
		{
			return (byte)((byteTileHeader & 0x60) >> 5);
		}

		public bool nactive()
		{
			if ((shortTileHeader & 0x60) == 32)
			{
				return true;
			}
			return false;
		}

		public void ResetToType(ushort type)
		{
			liquidCount = 0;
			shortTileHeader = 32;
			byteTileHeader = 0;
			byteTileHeader2 = 0;
			byteTileHeader3 = 0;
			this.tileType = type;
		}

		public void ClearMetadata()
		{
			liquidCount = 0;
			shortTileHeader = 0;
			byteTileHeader = 0;
			byteTileHeader2 = 0;
			byteTileHeader3 = 0;
		}

		public Color GetInactiveColor(Color oldColor)
		{
			if (!GetInactive())
			{
				return oldColor;
			}
			double num = 0.4;
			return new Color((byte)(num * (double)(int)oldColor.R), (byte)(num * (double)(int)oldColor.G), (byte)(num * (double)(int)oldColor.B), oldColor.A);
		}

		public void SetActiveColor(ref Vector3 oldColor)
		{
			if (GetInactive())
			{
				oldColor *= 0.4f;
			}
		}

		public bool GetTopSlope()
		{
			byte b = GetSlopeType();
			if (b != 1)
			{
				return b == 2;
			}
			return true;
		}

		public bool GetBottomSlope()
		{
			byte b = GetSlopeType();
			if (b != 3)
			{
				return b == 4;
			}
			return true;
		}

		public bool GetLeftSlope()
		{
			byte b = GetSlopeType();
			if (b != 2)
			{
				return b == 4;
			}
			return true;
		}

		public bool GetRightSlope()
		{
			byte b = GetSlopeType();
			if (b != 1)
			{
				return b == 3;
			}
			return true;
		}

		public bool SlopeEquals(Tile tile)
		{
			return (shortTileHeader & 0x7400) == (tile.shortTileHeader & 0x7400);
		}

		public byte GetWallPaint()
		{
			return (byte)(byteTileHeader & 0x1Fu);
		}

		public void SetWallPaint(byte wallColor)
		{
			byteTileHeader = (byte)((byteTileHeader & 0xE0u) | wallColor);
		}

		public bool LiquidIsLava()
		{
			return (byteTileHeader & 0x20) == 32;
		}

		public void SetIsLava(bool lava)
		{
			if (lava)
			{
				byteTileHeader = (byte)((byteTileHeader & 0x9Fu) | 0x20u);
			}
			else
			{
				byteTileHeader &= 223;
			}
		}

		public bool LiquidIsHoney()
		{
			return (byteTileHeader & 0x40) == 64;
		}

		public void SetIsHoney(bool honey)
		{
			if (honey)
			{
				byteTileHeader = (byte)((byteTileHeader & 0x9Fu) | 0x40u);
			}
			else
			{
				byteTileHeader &= 191;
			}
		}

		public bool GetWire4()
		{
			return (byteTileHeader & 0x80) == 128;
		}

		public void SetWire4(bool wire4)
		{
			if (wire4)
			{
				byteTileHeader |= 128;
			}
			else
			{
				byteTileHeader &= 127;
			}
		}

		public bool GetCheckingLiquid()
		{
			return (byteTileHeader3 & 8) == 8;
		}

		public void SetCheckingLiquid(bool checkingLiquid)
		{
			if (checkingLiquid)
			{
				byteTileHeader3 |= 8;
			}
			else
			{
				byteTileHeader3 &= 247;
			}
		}

		public bool GetSkipLiquid()
		{
			return (byteTileHeader3 & 0x10) == 16;
		}

		public void SetSkipLiquid(bool skipLiquid)
		{
			if (skipLiquid)
			{
				byteTileHeader3 |= 16;
			}
			else
			{
				byteTileHeader3 &= 239;
			}
		}

		public byte GetTilePaint()
		{
			return (byte)((uint)shortTileHeader & 0x1Fu);
		}

		public void SetTilePaint(byte color)
		{
			shortTileHeader = (short)((shortTileHeader & 0xFFE0) | color);
		}

		public bool GetTileActive()
		{
			return (shortTileHeader & 0x20) == 32;
		}

		public void SetTileActive(bool active)
		{
			if (active)
			{
				shortTileHeader |= 32;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xFFDF);
			}
		}

		public bool GetInactive()
		{
			return (shortTileHeader & 0x40) == 64;
		}

		public void SetInactive(bool inActive)
		{
			if (inActive)
			{
				shortTileHeader |= 64;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xFFBF);
			}
		}

		public bool GetWire()
		{
			return (shortTileHeader & 0x80) == 128;
		}

		public void SetWire(bool wire)
		{
			if (wire)
			{
				shortTileHeader |= 128;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xFF7F);
			}
		}

		public bool GetWire2()
		{
			return (shortTileHeader & 0x100) == 256;
		}

		public void SetWire2(bool wire2)
		{
			if (wire2)
			{
				shortTileHeader |= 256;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xFEFF);
			}
		}

		public bool GetWire3()
		{
			return (shortTileHeader & 0x200) == 512;
		}

		public void SetWire3(bool wire3)
		{
			if (wire3)
			{
				shortTileHeader |= 512;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xFDFF);
			}
		}

		public bool GetHalfBrick()
		{
			return (shortTileHeader & 0x400) == 1024;
		}

		public void SetHalfBrick(bool halfBrick)
		{
			if (halfBrick)
			{
				shortTileHeader |= 1024;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xFBFF);
			}
		}

		public bool GetActuator()
		{
			return (shortTileHeader & 0x800) == 2048;
		}

		public void SetActuator(bool actuator)
		{
			if (actuator)
			{
				shortTileHeader |= 2048;
			}
			else
			{
				shortTileHeader = (short)(shortTileHeader & 0xF7FF);
			}
		}

		public byte GetSlopeType()
		{
			return (byte)((shortTileHeader & 0x7000) >> 12);
		}

		public void SetSlopeType(byte slope)
		{
			shortTileHeader = (short)((shortTileHeader & 0x8FFF) | ((slope & 7) << 12));
		}

		public override string ToString()
		{
			return "Tile Type:" + tileType + " Active:" + GetTileActive() + " Wall:" + wallType + " Slope:" + GetSlopeType();
		}
	}
}
