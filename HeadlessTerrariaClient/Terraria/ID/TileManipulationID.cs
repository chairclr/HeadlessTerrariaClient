using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Terraria.ID
{
    public class TileManipulationID
    {
        // fail - flags1
        public const int KillTile = 0;
        // type - flags1, style - flags2
        public const int PlaceTile = 1;
        // fail - flags1
        public const int KillWall = 2;
        // type - flags1
        public const int PlaceWall = 3;
        // fail - flags1
        public const int KillTileNoItem = 4;
        public const int PlaceWire = 5;
        public const int KillWire = 6;
        public const int PoundTile = 7;
        public const int PlaceActuator = 8;
        public const int KillActuator = 9;
        public const int PlaceWire2 = 10;
        public const int KillWire2 = 11;
        public const int PlaceWire3 = 12;
        public const int KillWire3 = 13;
        // slope - flags1
        public const int SlopeTile = 14;
        public const int FrameTrack = 15;
        public const int PlaceWire4 = 16;
        public const int KillWire4 = 17;
        public const int PokeLogicGate = 18;
        public const int Actuate = 19;
        public const int KillTile2 = 20;
        // type - flags1, style - flags2
        public const int ReplaceTile = 21;
        public const int ReplaceWall = 22;
        public const int SlopePoundTile = 23;
    }

    public class TileManipulation
    {
        public int action;
        public int tileX;
        public int tileY;
        public int flags;
        public int flags2;

        public TileManipulation(int action, int tileX, int tileY, int flags, int flags2)
        {
            this.action = action;
            this.tileX = tileX;
            this.tileY = tileY;
            this.flags = flags;
            this.flags2 = flags2;
        }
    }
}
