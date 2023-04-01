using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Game;

public enum TileManipulationType
{
    KillTile = 0,
    PlaceTile = 1,
    KillWall = 2,
    PlaceWall = 3,
    KillTileNoItem = 4,
    PlaceWire = 5,
    KillWire = 6,
    PoundTile = 7,
    PlaceActuator = 8,
    KillActuator = 9,
    PlaceWire2 = 10,
    KillWire2 = 11,
    PlaceWire3 = 12,
    KillWire3 = 13,
    SlopeTile = 14,
    FrameTrack = 15,
    PlaceWire4 = 16,
    KillWire4 = 17,
    PokeLogicGate = 18,
    Actuate = 19,
    TryKillTile = 20,
    ReplaceTile = 21,
    ReplaceWall = 22,
    SlopePoundTile = 23
}
