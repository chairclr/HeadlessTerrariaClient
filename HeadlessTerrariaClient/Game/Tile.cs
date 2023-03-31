using System.ComponentModel.Design;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HeadlessTerrariaClient.Game;

public unsafe struct Tile
{
    public readonly TileData* Data;
    public readonly ref TileData RefData => ref Unsafe.AsRef<TileData>(Data);
    public ref ushort Type => ref Data->Type;
    public ref ushort Wall => ref Data->Wall ;
    public ref ushort TileHeader => ref Data->TileHeader ;
    public ref short FrameX => ref Data->FrameX ;
    public ref short FrameY => ref Data->FrameY ;
    public ref byte LiquidAmount => ref Data->LiquidAmount ;
    public ref byte TileHeader1 => ref Data->TileHeader1 ;
    public ref byte TileHeader2 => ref Data->TileHeader2 ;
    public ref byte TileHeader3 => ref Data->TileHeader3 ;

    public Tile()
    {
        Data = null;
    }
    public Tile(ref TileData data)
    {
        Data = (TileData*)Unsafe.AsPointer(ref data);
    }
    public Tile(TileData* data)
    {
        Data = data;
    }
    public override int GetHashCode() => HashCode.Combine(*Data);
    private const int Bit0 = 1;
    private const int Bit1 = 2;
    private const int Bit2 = 4;
    private const int Bit3 = 8;
    private const int Bit4 = 16;
    private const int Bit5 = 32;
    private const int Bit6 = 64;
    private const int Bit7 = 128;
    private const ushort Bit15 = 32768;
    public const int Type_Solid = 0;
    public const int Type_Halfbrick = 1;
    public const int Type_SlopeDownRight = 2;
    public const int Type_SlopeDownLeft = 3;
    public const int Type_SlopeUpRight = 4;
    public const int Type_SlopeUpLeft = 5;
    public const int Liquid_Water = 0;
    public const int Liquid_Lava = 1;
    public const int Liquid_Honey = 2;
    public const int Liquid_Shimmer = 3;
    private const int NeitherLavaOrHoney = 159;
    private const int EitherLavaOrHoney = 96;

    public void ClearEverything() => Data->ClearEverything();

    public void ClearTile() => Data->ClearTile();

    public void CopyFrom(Tile from) => Data->CopyFrom(ref from.RefData);

    public void CopyFrom(ref TileData from) => Data->CopyFrom(ref from);

    public unsafe bool IsTheSameAs(Tile compTile) => IsTheSameAs(ref compTile.RefData);

    public bool IsTheSameAs(ref TileData compTile) => Data->IsTheSameAs(ref compTile);

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
                    Lava = (true);
                    break;
                case 2:
                    Honey = (true);
                    break;
                case 3:
                    Shimmer = (true);
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

    public void CopyPaintAndCoating(Tile other) => Data->CopyPaintAndCoating(ref other.RefData);

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

    public void UseBlockColors(TileColorCache cache)
    {
        cache.ApplyToBlock(this);
    }

    public void UseWallColors(TileColorCache cache)
    {
        cache.ApplyToWall(this);
    }

    public void ClearBlockPaintAndCoating()
    {
        Color = 0;
        FullbrightBlock = (false);
        InvisibleBlock = (false);
    }

    public void ClearWallPaintAndCoating()
    {
        WallColor = (0);
        FullbrightWall = (false);
        InvisibleWall = (false);
    }

    public override string ToString()
    {
        return "Tile Type:" + Type + " Active:" + Active.ToString() + " Wall:" + Wall + " Slope:" + Slope + " fX:" + FrameX + " fY:" + FrameY;
    }
}

public struct TileColorCache
{
    public byte Color;
    public bool FullBright;
    public bool Invisible;
    public void ApplyToBlock(Tile tile)
    {
        tile.Color = (Color);
        tile.FullbrightBlock = (FullBright);
        tile.InvisibleBlock = (Invisible);
    }

    public void ApplyToWall(Tile tile)
    {
        tile.WallColor = (Color);
        tile.FullbrightWall = (FullBright);
        tile.InvisibleWall = (Invisible);
    }
}