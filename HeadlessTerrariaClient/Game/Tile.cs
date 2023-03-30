using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Game;

public unsafe struct Tile
{
    public readonly TileData* Data;
    public readonly ref TileData RefData => ref Unsafe.AsRef<TileData>(Data);
    public ref ushort type => ref Data->type;
    public ref ushort wall => ref Data->wall;
    public ref short frameX => ref Data->frameX;
    public ref short frameY => ref Data->frameY;
    public ref ushort sTileHeader => ref Data->sTileHeader;
    public ref byte liquid => ref Data->liquid;
    public ref byte bTileHeader => ref Data->bTileHeader;
    public ref byte bTileHeader2 => ref Data->bTileHeader2;
    public ref byte bTileHeader3 => ref Data->bTileHeader3;
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

    public void CopyFrom(Tile from)
    {
        *Data = *from.Data;
    }

    public bool isTheSameAs(Tile compTile) => Data->isTheSameAs(compTile);

    public int blockType()
    {
        if (halfBrick())
        {
            return 1;
        }

        int num = slope();
        if (num > 0)
        {
            num++;
        }

        return num;
    }

    public void liquidType(int liquidType)
    {
        switch (liquidType)
        {
            case 0:
                bTileHeader &= 159;
                break;
            case 1:
                lava(lava: true);
                break;
            case 2:
                honey(honey: true);
                break;
            case 3:
                shimmer(shimmer: true);
                break;
        }
    }

    public byte liquidType()
    {
        return (byte)((bTileHeader & 0x60) >> 5);
    }

    public bool nactive()
    {
        if ((sTileHeader & 0x60) == 32)
        {
            return true;
        }

        return false;
    }

    public void ResetToType(ushort type) => Data->ResetToType(type);

    internal void ClearMetadata() => Data->ClearMetadata();

    public Color actColor(Color oldColor)
    {
        if (!inActive())
        {
            return oldColor;
        }

        double num = 0.4;
        return new Color((byte)(num * (double)(int)oldColor.R), (byte)(num * (double)(int)oldColor.G), (byte)(num * (double)(int)oldColor.B), oldColor.A);
    }

    public void actColor(ref Vector3 oldColor)
    {
        if (inActive())
        {
            oldColor *= 0.4f;
        }
    }

    public bool topSlope()
    {
        byte b = slope();
        if (b != 1)
        {
            return b == 2;
        }

        return true;
    }

    public bool bottomSlope()
    {
        byte b = slope();
        if (b != 3)
        {
            return b == 4;
        }

        return true;
    }

    public bool leftSlope()
    {
        byte b = slope();
        if (b != 2)
        {
            return b == 4;
        }

        return true;
    }

    public bool rightSlope()
    {
        byte b = slope();
        if (b != 1)
        {
            return b == 3;
        }

        return true;
    }

    public bool HasSameSlope(Tile tile)
    {
        return (sTileHeader & 0x7400) == (tile.sTileHeader & 0x7400);
    }

    public byte wallColor()
    {
        return (byte)(bTileHeader & 0x1Fu);
    }

    public void wallColor(byte wallColor)
    {
        bTileHeader = (byte)((bTileHeader & 0xE0u) | wallColor);
    }

    public bool lava()
    {
        return (bTileHeader & 0x60) == 32;
    }

    public void lava(bool lava)
    {
        if (lava)
        {
            bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x20u);
        }
        else
        {
            bTileHeader &= 223;
        }
    }

    public bool honey()
    {
        return (bTileHeader & 0x60) == 64;
    }

    public void honey(bool honey)
    {
        if (honey)
        {
            bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x40u);
        }
        else
        {
            bTileHeader &= 191;
        }
    }

    public bool shimmer()
    {
        return (bTileHeader & 0x60) == 96;
    }

    public void shimmer(bool shimmer)
    {
        if (shimmer)
        {
            bTileHeader = (byte)((bTileHeader & 0x9Fu) | 0x60u);
        }
        else
        {
            bTileHeader &= 159;
        }
    }

    public bool wire4()
    {
        return (bTileHeader & 0x80) == 128;
    }

    public void wire4(bool wire4)
    {
        if (wire4)
        {
            bTileHeader |= 128;
        }
        else
        {
            bTileHeader &= 127;
        }
    }

    public int wallFrameX()
    {
        return (bTileHeader2 & 0xF) * 36;
    }

    public void wallFrameX(int wallFrameX)
    {
        bTileHeader2 = (byte)((bTileHeader2 & 0xF0u) | ((uint)(wallFrameX / 36) & 0xFu));
    }

    public byte frameNumber()
    {
        return (byte)((bTileHeader2 & 0x30) >> 4);
    }

    public void frameNumber(byte frameNumber)
    {
        bTileHeader2 = (byte)((bTileHeader2 & 0xCFu) | (uint)((frameNumber & 3) << 4));
    }

    public byte wallFrameNumber()
    {
        return (byte)((bTileHeader2 & 0xC0) >> 6);
    }

    public void wallFrameNumber(byte wallFrameNumber)
    {
        bTileHeader2 = (byte)((bTileHeader2 & 0x3Fu) | (uint)((wallFrameNumber & 3) << 6));
    }

    public int wallFrameY()
    {
        return (bTileHeader3 & 7) * 36;
    }

    public void wallFrameY(int wallFrameY)
    {
        bTileHeader3 = (byte)((bTileHeader3 & 0xF8u) | ((uint)(wallFrameY / 36) & 7u));
    }

    public bool checkingLiquid()
    {
        return (bTileHeader3 & 8) == 8;
    }

    public void checkingLiquid(bool checkingLiquid)
    {
        if (checkingLiquid)
        {
            bTileHeader3 |= 8;
        }
        else
        {
            bTileHeader3 &= 247;
        }
    }

    public bool skipLiquid()
    {
        return (bTileHeader3 & 0x10) == 16;
    }

    public void skipLiquid(bool skipLiquid)
    {
        if (skipLiquid)
        {
            bTileHeader3 |= 16;
        }
        else
        {
            bTileHeader3 &= 239;
        }
    }

    public bool invisibleBlock()
    {
        return (bTileHeader3 & 0x20) == 32;
    }

    public void invisibleBlock(bool invisibleBlock)
    {
        if (invisibleBlock)
        {
            bTileHeader3 |= 32;
        }
        else
        {
            bTileHeader3 = (byte)(bTileHeader3 & 0xFFFFFFDFu);
        }
    }

    public bool invisibleWall()
    {
        return (bTileHeader3 & 0x40) == 64;
    }

    public void invisibleWall(bool invisibleWall)
    {
        if (invisibleWall)
        {
            bTileHeader3 |= 64;
        }
        else
        {
            bTileHeader3 = (byte)(bTileHeader3 & 0xFFFFFFBFu);
        }
    }

    public bool fullbrightBlock()
    {
        return (bTileHeader3 & 0x80) == 128;
    }

    public void fullbrightBlock(bool fullbrightBlock)
    {
        if (fullbrightBlock)
        {
            bTileHeader3 |= 128;
        }
        else
        {
            bTileHeader3 = (byte)(bTileHeader3 & 0xFFFFFF7Fu);
        }
    }

    public byte color()
    {
        return (byte)(sTileHeader & 0x1Fu);
    }

    public void color(byte color)
    {
        sTileHeader = (ushort)((sTileHeader & 0xFFE0u) | color);
    }

    public bool active()
    {
        return (sTileHeader & 0x20) == 32;
    }

    public void active(bool active)
    {
        if (active)
        {
            sTileHeader |= 32;
        }
        else
        {
            sTileHeader &= 65503;
        }
    }

    public bool inActive()
    {
        return (sTileHeader & 0x40) == 64;
    }

    public void inActive(bool inActive)
    {
        if (inActive)
        {
            sTileHeader |= 64;
        }
        else
        {
            sTileHeader &= 65471;
        }
    }

    public bool wire()
    {
        return (sTileHeader & 0x80) == 128;
    }

    public void wire(bool wire)
    {
        if (wire)
        {
            sTileHeader |= 128;
        }
        else
        {
            sTileHeader &= 65407;
        }
    }

    public bool wire2()
    {
        return (sTileHeader & 0x100) == 256;
    }

    public void wire2(bool wire2)
    {
        if (wire2)
        {
            sTileHeader |= 256;
        }
        else
        {
            sTileHeader &= 65279;
        }
    }

    public bool wire3()
    {
        return (sTileHeader & 0x200) == 512;
    }

    public void wire3(bool wire3)
    {
        if (wire3)
        {
            sTileHeader |= 512;
        }
        else
        {
            sTileHeader &= 65023;
        }
    }

    public bool halfBrick()
    {
        return (sTileHeader & 0x400) == 1024;
    }

    public void halfBrick(bool halfBrick)
    {
        if (halfBrick)
        {
            sTileHeader |= 1024;
        }
        else
        {
            sTileHeader &= 64511;
        }
    }

    public bool actuator()
    {
        return (sTileHeader & 0x800) == 2048;
    }

    public void actuator(bool actuator)
    {
        if (actuator)
        {
            sTileHeader |= 2048;
        }
        else
        {
            sTileHeader &= 63487;
        }
    }

    public byte slope()
    {
        return (byte)((sTileHeader & 0x7000) >> 12);
    }

    public void slope(byte slope)
    {
        sTileHeader = (ushort)((sTileHeader & 0x8FFFu) | (uint)((slope & 7) << 12));
    }

    public bool fullbrightWall()
    {
        return (sTileHeader & 0x8000) == 32768;
    }

    public void fullbrightWall(bool fullbrightWall)
    {
        if (fullbrightWall)
        {
            sTileHeader |= 32768;
        }
        else
        {
            sTileHeader = (ushort)(sTileHeader & 0xFFFF7FFFu);
        }
    }


    public void CopyPaintAndCoating(Tile other) => Data->CopyPaintAndCoating(ref other.RefData);

    public TileColorCache BlockColorAndCoating()
    {
        TileColorCache result = default(TileColorCache);
        result.Color = color();
        result.FullBright = fullbrightBlock();
        result.Invisible = invisibleBlock();
        return result;
    }

    public TileColorCache WallColorAndCoating()
    {
        TileColorCache result = default(TileColorCache);
        result.Color = wallColor();
        result.FullBright = fullbrightWall();
        result.Invisible = invisibleWall();
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
        color(0);
        fullbrightBlock(fullbrightBlock: false);
        invisibleBlock(invisibleBlock: false);
    }

    public void ClearWallPaintAndCoating()
    {
        wallColor(0);
        fullbrightWall(fullbrightWall: false);
        invisibleWall(invisibleWall: false);
    }

    public override string ToString()
    {
        return "Tile Type:" + type + " Active:" + active().ToString() + " Wall:" + wall + " Slope:" + slope() + " fX:" + frameX + " fY:" + frameY;
    }
}

public struct TileColorCache
{
    public byte Color;
    public bool FullBright;
    public bool Invisible;
    public void ApplyToBlock(Tile tile)
    {
        tile.color(Color);
        tile.fullbrightBlock(FullBright);
        tile.invisibleBlock(Invisible);
    }

    public void ApplyToWall(Tile tile)
    {
        tile.wallColor(Color);
        tile.fullbrightWall(FullBright);
        tile.invisibleWall(Invisible);
    }
}