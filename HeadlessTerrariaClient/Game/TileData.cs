using System;
using System.Drawing;
using System.Numerics;
using static System.Formats.Asn1.AsnWriter;

namespace HeadlessTerrariaClient.Game;

public struct TileData
{
    public ushort Type;
    public ushort Wall;
    public ushort TileHeader;
    public short FrameX;
    public short FrameY;
    public byte LiquidAmount;
    public byte TileHeader1;
    public byte TileHeader2;
    public byte TileHeader3;

    public void ClearEverything()
    {
        this = new TileData();
    }

    public void ClearTile()
    {
        Slope = 0;
        HalfBrick = false;
        Active = false;
        InActive = false;
    }

    public void CopyFrom(ref TileData from)
    {
        this = from;
    }

    public unsafe bool IsTheSameAs(Tile compTile) => IsTheSameAs(ref compTile.RefData);

    public bool IsTheSameAs(ref TileData compTile)
    {
        if (TileHeader != compTile.TileHeader)
        {
            return false;
        }

        if (Active)
        {
            if (Type != compTile.Type)
            {
                return false;
            }

            if (false && (FrameX != compTile.FrameX || FrameY != compTile.FrameY))
            {
                return false;
            }
        }

        if (Wall != compTile.Wall || LiquidAmount != compTile.LiquidAmount)
        {
            return false;
        }

        if (compTile.LiquidAmount == 0)
        {
            if (WallColor != compTile.WallColor)
            {
                return false;
            }

            if (Wire4 != compTile.Wire4)
            {
                return false;
            }
        }
        else if (TileHeader1 != compTile.TileHeader1)
        {
            return false;
        }

        if (InvisibleBlock != compTile.InvisibleBlock || InvisibleWall != compTile.InvisibleWall || FullbrightBlock != compTile.FullbrightBlock || FullbrightWall != compTile.FullbrightWall)
        {
            return false;
        }

        return true;
    }

    public int BlockType
    {
        get
        {
            if (HalfBrick)
            {
                return 1;
            }

            int num = Slope;
            if (num > 0)
            {
                num++;
            }

            return num;
        }
    }

    public int LiquidType
    {
        get => (byte)((TileHeader1 & 0x60) >> 5);
        set
        {
            switch (value)
            {
                case 0:
                    TileHeader1 &= 159;
                    break;
                case 1:
                    Lava = true;
                    break;
                case 2:
                    Honey = true;
                    break;
                case 3:
                    Shimmer = true;
                    break;
                default:
                    break;
            }
        }
    }

    public bool NActive
    {
        get
        {
            if ((TileHeader & 0x60) == 32)
            {
                return true;
            }

            return false;
        }
    }

    public void ResetToType(ushort type)
    {
        LiquidAmount = 0;
        TileHeader = 32;
        TileHeader1 = 0;
        TileHeader2 = 0;
        TileHeader3 = 0;
        FrameX = 0;
        FrameY = 0;
        this.Type = type;
    }

    public void ClearMetadata()
    {
        LiquidAmount = 0;
        TileHeader = 0;
        TileHeader1 = 0;
        TileHeader2 = 0;
        TileHeader3 = 0;
        FrameX = 0;
        FrameY = 0;
    }

    public Color ActColor(Color oldColor)
    {
        if (!InActive)
        {
            return oldColor;
        }

        double num = 0.4;
        return new Color((byte)(num * (double)(int)oldColor.R), (byte)(num * (double)(int)oldColor.G), (byte)(num * (double)(int)oldColor.B), oldColor.A);
    }

    public void ActColor(ref Vector3 oldColor)
    {
        if (InActive)
        {
            oldColor *= 0.4f;
        }
    }

    public bool TopSlope
    {
        get
        {
            byte b = Slope;
            if (b != 1)
            {
                return b == 2;
            }

            return true;
        }
    }

    public bool BottomSlope
    {
        get
        {
            byte b = Slope;
            if (b != 3)
            {
                return b == 4;
            }

            return true;
        }
    }

    public bool LeftSlope
    {
        get
        {
            byte b = Slope;
            if (b != 2)
            {
                return b == 4;
            }

            return true;
        }
    }

    public bool RightSlope
    {
        get
        {
            byte b = Slope;
            if (b != 1)
            {
                return b == 3;
            }

            return true;
        }
    }

    public bool HasSameSlope(ref TileData tile)
    {
        return (TileHeader & 0x7400) == (tile.TileHeader & 0x7400);
    }

    public byte WallColor
    {
        get => (byte)(TileHeader1 & 0x1Fu);
        set => TileHeader1 = (byte)((TileHeader1 & 0xE0u) | value);
    }

    public bool Lava
    {
        get => (TileHeader1 & 0x60) == 32;
        set
        {
            if (value)
            {
                TileHeader1 = (byte)((TileHeader1 & 0x9Fu) | 0x20u);
            }
            else
            {
                TileHeader1 &= 223;
            }
        }
    }

    public bool Honey
    {
        get => (TileHeader1 & 0x60) == 64;
        set
        {
            if (value)
            {
                TileHeader1 = (byte)((TileHeader1 & 0x9Fu) | 0x40u);
            }
            else
            {
                TileHeader1 &= 191;
            }
        }
    }

    public bool Shimmer
    {
        get => (TileHeader1 & 0x60) == 96;
        set
        {
            if (value)
            {
                TileHeader1 = (byte)((TileHeader1 & 0x9Fu) | 0x60u);
            }
            else
            {
                TileHeader1 &= 159;
            }
        }
    }

    public bool Wire4
    {
        get => (TileHeader1 & 0x80) == 128;
        set
        {
            if (value)
            {
                TileHeader1 |= 128;
            }
            else
            {
                TileHeader1 &= 127;
            }
        }
    }

    public int WallFrameX
    {
        get => (TileHeader2 & 0xF) * 36;
        set
        {
            TileHeader2 = (byte)((TileHeader2 & 0xF0u) | ((uint)(value / 36) & 0xFu));
        }
    }

    public byte FrameNumber
    {
        get => (byte)((TileHeader2 & 0x30) >> 4);
        set
        {
            TileHeader2 = (byte)((TileHeader2 & 0xCFu) | (uint)((value & 3) << 4));
        }
    }

    public byte WallFrameNumber
    {
        get => (byte)((TileHeader2 & 0xC0) >> 6);
        set
        {
            TileHeader2 = (byte)((TileHeader2 & 0x3Fu) | (uint)((value & 3) << 6));
        }
    }

    public int WallFrameY
    {
        get => (TileHeader3 & 7) * 36;
        set
        {
            TileHeader3 = (byte)((TileHeader3 & 0xF8u) | ((uint)(value / 36) & 7u));
        }
    }

    public bool CheckingLiquid
    {
        get => (TileHeader3 & 8) == 8;
        set
        {
            if (value)
            {
                TileHeader3 |= 8;
            }
            else
            {
                TileHeader3 &= 247;
            }
        }
    }

    public bool SkipLiquid
    {
        get => (TileHeader3 & 0x10) == 16;
        set
        {
            if (value)
            {
                TileHeader3 |= 16;
            }
            else
            {
                TileHeader3 &= 239;
            }
        }
    }

    public bool InvisibleBlock
    {
        get => (TileHeader3 & 0x20) == 32;
        set
        {
            if (value)
            {
                TileHeader3 |= 32;
            }
            else
            {
                TileHeader3 = (byte)(TileHeader3 & 0xFFFFFFDFu);
            }
        }
    }

    public bool InvisibleWall
    {
        get => (TileHeader3 & 0x40) == 64;
        set
        {
            if (value)
            {
                TileHeader3 |= 64;
            }
            else
            {
                TileHeader3 = (byte)(TileHeader3 & 0xFFFFFFBFu);
            }
        }
    }

    public bool FullbrightBlock
    {
        get => (TileHeader3 & 0x80) == 128;
        set
        {
            if (value)
            {
                TileHeader3 |= 128;
            }
            else
            {
                TileHeader3 = (byte)(TileHeader3 & 0xFFFFFF7Fu);
            }
        }
    }

    public byte Color
    {
        get => (byte)(TileHeader & 0x1Fu);
        set
        {
            TileHeader = (ushort)((TileHeader & 0xFFE0u) | value);
        }
    }

    public bool Active
    {
        get => (TileHeader & 0x20) == 32;
        set
        {
            if (value)
            {
                TileHeader |= 32;
            }
            else
            {
                TileHeader &= 65503;
            }
        }
    }

    public bool InActive
    {
        get => (TileHeader & 0x40) == 64;
        set
        {
            if (value)
            {
                TileHeader |= 64;
            }
            else
            {
                TileHeader &= 65471;
            }
        }
    }

    public bool Wire
    {
        get => (TileHeader & 0x80) == 128;
        set
        {
            if (value)
            {
                TileHeader |= 128;
            }
            else
            {
                TileHeader &= 65407;
            }
        }
    }

    public bool Wire2
    {
        get => (TileHeader & 0x100) == 256;
        set
        {
            if (value)
            {
                TileHeader |= 256;
            }
            else
            {
                TileHeader &= 65279;
            }
        }
    }

    public bool Wire3
    {
        get => (TileHeader & 0x200) == 512;
        set
        {
            if (value)
            {
                TileHeader |= 512;
            }
            else
            {
                TileHeader &= 65023;
            }
        }
    }

    public bool HalfBrick
    {
        get => (TileHeader & 0x400) == 1024;
        set
        {
            if (value)
            {
                TileHeader |= 1024;
            }
            else
            {
                TileHeader &= 64511;
            }
        }
    }

    public bool Actuator
    {
        get => (TileHeader & 0x800) == 2048;
        set
        {
            if (value)
            {
                TileHeader |= 2048;
            }
            else
            {
                TileHeader &= 63487;
            }
        }
    }

    public byte Slope
    {
        get => (byte)((TileHeader & 0x7000) >> 12);
        set
        {
            TileHeader = (ushort)((TileHeader & 0x8FFFu) | (uint)((value & 7) << 12));
        }
    }

    public bool FullbrightWall
    {
        get => (TileHeader & 0x8000) == 32768;
        set
        {
            if (value)
            {
                TileHeader |= 32768;
            }
            else
            {
                TileHeader = (ushort)(TileHeader & 0xFFFF7FFFu);
            }
        }
    }

    public void CopyPaintAndCoating(Tile other)
    {
        Color = other.Color;
        InvisibleBlock = other.InvisibleBlock;
        FullbrightBlock = other.FullbrightBlock;
    }

    public void CopyPaintAndCoating(ref TileData other)
    {
        Color = other.Color;
        InvisibleBlock = other.InvisibleBlock;
        FullbrightBlock = other.FullbrightBlock;
    }

    public TileColorCache BlockColorAndCoating()
    {
        TileColorCache result = default(TileColorCache);
        result.Color = Color;
        result.FullBright = FullbrightBlock;
        result.Invisible = InvisibleBlock;
        return result;
    }

    public TileColorCache WallColorAndCoating()
    {
        TileColorCache result = default(TileColorCache);
        result.Color = WallColor;
        result.FullBright = FullbrightWall;
        result.Invisible = InvisibleWall;
        return result;
    }

    public void ClearBlockPaintAndCoating()
    {
        Color = 0;
        FullbrightBlock = false;
        InvisibleBlock = false;
    }

    public void ClearWallPaintAndCoating()
    {
        WallColor = 0;
        FullbrightWall = false;
        InvisibleWall = false;
    }

    public override string ToString()
    {
        return "Tile Type:" + Type + " Active:" + Active.ToString() + " Wall:" + Wall + " Slope:" + Slope + " fX:" + FrameX + " fY:" + FrameY;
    }
}
