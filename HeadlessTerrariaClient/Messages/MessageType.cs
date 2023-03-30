﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Messages;

public enum MessageType
{
    NeverCalled = 0,
    Hello = 1,
    Kick = 2,
    PlayerInfo = 3,
    SyncPlayer = 4,
    SyncEquipment = 5,
    RequestWorldData = 6,
    WorldData = 7,
    SpawnTileData = 8,
    StatusTextSize = 9,
    TileSection = 10,
    TileFrameSection = 11,
    PlayerSpawn = 12,
    PlayerControls = 13,
    PlayerActive = 14,
    Unknown15 = 15,
    PlayerLife = 16,
    TileManipulation = 17,
    SetTime = 18,
    ToggleDoorState = 19,
    SendTileSquare = 20,
    SyncItem = 21,
    ItemOwner = 22,
    SyncNPC = 23,
    StrikeNPCWithHeldItem = 24,
    Unused25 = 25,
    Unused26 = 26,
    SyncProjectile = 27,
    DamageNPC = 28,
    KillProjectile = 29,
    TogglePVP = 30,
    RequestChestOpen = 31,
    SyncChestItem = 32,
    SyncPlayerChest = 33,
    ChestUpdates = 34,
    PlayerHeal = 35,
    SyncPlayerZone = 36,
    RequestPassword = 37,
    SendPassword = 38,
    ReleaseItemOwnership = 39,
    SyncTalkNPC = 40,
    ShotAnimationAndSound = 41,
    PlayerMana = 42,
    ManaEffect = 43,
    Unknown44 = 44,
    PlayerTeam = 45,
    RequestSign = 46,
    UpdateSign = 47,
    LiquidUpdate = 48,
    InitialSpawn = 49,
    PlayerBuffs = 50,
    MiscDataSync = 51,
    LockAndUnlock = 52,
    AddNPCBuff = 53,
    NPCBuffs = 54,
    AddPlayerBuff = 55,
    UniqueTownNPCInfoSyncRequest = 56,
    UpdateWorldEvil = 57,
    InstrumentSound = 58,
    HitSwitch = 59,
    UpdateNPCHome = 60,
    SpawnBossUseLicenseStartEvent = 61,
    PlayerDodge = 62,
    PaintTile = 63,
    PaintWall = 64,
    TeleportEntity = 65,
    PlayerNPCTeleport = 66,
    Unknown67 = 67,
    ClientUUID = 68,
    ChestName = 69,
    BugCatching = 70,
    BugReleasing = 71,
    TravelMerchantItems = 72,
    RequestTeleportationByServer = 73,
    AnglerQuest = 74,
    AnglerQuestFinished = 75,
    QuestsCountSync = 76,
    TemporaryAnimation = 77,
    InvasionProgressReport = 78,
    PlaceObject = 79,
    SyncPlayerChestIndex = 80,
    CombatTextInt = 81,
    NetModules = 82,
    NPCKillCountDeathTally = 83,
    PlayerStealth = 84,
    QuickStackChests = 85,
    TileEntitySharing = 86,
    TileEntityPlacement = 87,
    ItemTweaker = 88,
    ItemFrameTryPlacing = 89,
    InstancedItem = 90,
    SyncEmoteBubble = 91,
    SyncExtraValue = 92,
    SocialHandshake = 93,
    Deprecated1 = 94,
    MurderSomeoneElsesPortal = 95,
    TeleportPlayerThroughPortal = 96,
    AchievementMessageNPCKilled = 97,
    AchievementMessageEventHappened = 98,
    MinionRestTargetUpdate = 99,
    TeleportNPCThroughPortal = 100,
    UpdateTowerShieldStrengths = 101,
    NebulaLevelupRequest = 102,
    MoonlordHorror = 103,
    ShopOverride = 104,
    GemLockToggle = 105,
    PoofOfSmoke = 106,
    SmartTextMessage = 107,
    WiredCannonShot = 108,
    MassWireOperation = 109,
    MassWireOperationPay = 110,
    ToggleParty = 111,
    SpecialFX = 112,
    CrystalInvasionStart = 113,
    CrystalInvasionWipeAllTheThingsss = 114,
    MinionAttackTargetUpdate = 115,
    CrystalInvasionSendWaitTime = 116,
    PlayerHurtV2 = 117,
    PlayerDeathV2 = 118,
    CombatTextString = 119,
    Emoji = 120,
    TEDisplayDollItemSync = 121,
    RequestTileEntityInteraction = 122,
    WeaponsRackTryPlacing = 123,
    TEHatRackItemSync = 124,
    SyncTilePicking = 125,
    SyncRevengeMarker = 126,
    RemoveRevengeMarker = 127,
    LandGolfBallInCup = 128,
    FinishedConnectingToServer = 129,
    FishOutNPC = 130,
    TamperWithNPC = 131,
    PlayLegacySound = 132,
    FoodPlatterTryPlacing = 133,
    UpdatePlayerLuckFactors = 134,
    DeadPlayer = 135,
    SyncCavernMonsterType = 136,
    RequestNPCBuffRemoval = 137,
    ClientSyncedInventory = 138,
    SetCountsAsHostForGameplay = 139,
    SetMiscEventValues = 140,
    RequestLucyPopup = 141,
    SyncProjectileTrackers = 142,
    CrystalInvasionRequestedToSkipWaitTime = 143,
    RequestQuestEffect = 144,
    SyncItemsWithShimmer = 145,
    ShimmerActions = 146,
    SyncLoadout = 147,
    SyncItemCannotBeTakenByEnemies = 148,
    Count = 149
}