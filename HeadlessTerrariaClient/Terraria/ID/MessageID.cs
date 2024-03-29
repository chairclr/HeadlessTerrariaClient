﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
namespace HeadlessTerrariaClient.Terraria.ID
{
	public class MessageID
	{
		private static string[] fieldNames = typeof(MessageID).GetFields().Select(x => x.Name).ToArray();

		public static string GetName(int id)
        {
			if (id < 0 || id >= fieldNames.Length)
				return "null";
			return fieldNames[id];
        }

		public const byte NeverCalled = 0;

		public const byte Hello = 1;

		public const byte Kick = 2;

		public const byte PlayerInfo = 3;

		public const byte SyncPlayer = 4;

		public const byte SyncEquipment = 5;

		public const byte RequestWorldData = 6;

		public const byte WorldData = 7;

		public const byte SpawnTileData = 8;

		public const byte StatusText = 9;

		public const byte TileSection = 10;

		public const byte TileFrameSection = 11;

		public const byte PlayerSpawn = 12;

		public const byte PlayerControls = 13;

		public const byte PlayerActive = 14;

		public const byte Unknown15 = 15;

		public const byte PlayerLife = 16;

		public const byte TileManipulation = 17;

		public const byte SetTime = 18;

		public const byte ToggleDoorState = 19;

		public const byte SendTileSquare = 20;

		public const byte SyncItem = 21;

		public const byte ItemOwner = 22;

		public const byte SyncNPC = 23;

		public const byte UnusedMeleeStrike = 24;

		public const byte Null25 = 25;

		public const byte Null26 = 26;

		public const byte SyncProjectile = 27;

		public const byte DamageNPC = 28;

		public const byte KillProjectile = 29;

		public const byte TogglePVP = 30;

		public const byte RequestChestOpen = 31;

		public const byte SyncChestItem = 32;

		public const byte SyncPlayerChest = 33;

		public const byte PlaceChest = 34;

		public const byte PlayerHeal = 35;

		public const byte SyncPlayerZone = 36;

		public const byte RequestPassword = 37;

		public const byte SendPassword = 38;

		public const byte ReleaseItemOwnership = 39;

		public const byte SyncTalkNPC = 40;

		public const byte SyncPlayerItemRotation = 41;

		public const byte PlayerMana = 42;

		public const byte ManaEffect = 43;

		public const byte Null44 = 44;

		public const byte PlayerTeam = 45;

		public const byte RequestSign = 46;

		public const byte UpdateSign = 47;

		public const byte LiquidUpdate = 48;

		public const byte CompleteConnectionAndSpawn = 49;

		public const byte SyncPlayerBuffs = 50;

		public const byte MiscDataSync = 51;

		public const byte UnlockTile = 52;

		public const byte AddNPCBuff = 53;

		public const byte UpdateNPCBuff = 54;

		public const byte AddPlayerBuff = 55;

		public const byte UniqueTownNPCInfoSyncRequest = 56;

		public const byte UpdateGoodEvil = 57;

		public const byte InstrumentSound = 58;

		public const byte HitSwitch = 59;

		public const byte UpdateNPCHome = 60;

		public const byte SpawnBoss = 61;

		public const byte PlayerDodge = 62;

		public const byte PaintTile = 63;

		public const byte PaintWall = 64;

		public const byte TeleportEntity = 65;

		public const byte HealOtherPlayer = 66;

		public const byte Placeholder = 67;

		public const byte ClientUUID = 68;

		public const byte ChestName = 69;

		public const byte BugCatching = 70;

		public const byte BugReleasing = 71;

		public const byte TravelMerchantItems = 72;

		public const byte RequestTeleportationByServer = 73;

		public const byte AnglerQuest = 74;

		public const byte AnglerQuestFinished = 75;

		public const byte QuestsCountSync = 76;

		public const byte TemporaryAnimation = 77;

		public const byte InvasionProgressReport = 78;

		public const byte PlaceObject = 79;

		public const byte SyncPlayerChestIndex = 80;

		public const byte CombatTextInt = 81;

		public const byte NetModules = 82;

		public const byte NPCKillCountDeathTally = 83;

		public const byte PlayerStealth = 84;

		public const byte QuickStackChests = 85;

		public const byte TileEntitySharing = 86;

		public const byte TileEntityPlacement = 87;

		public const byte ItemTweaker = 88;

		public const byte ItemFrameTryPlacing = 89;

		public const byte InstancedItem = 90;

		public const byte SyncEmoteBubble = 91;

		public const byte SyncExtraValue = 92;

		public const byte SocialHandshake = 93;

		public const byte Deprecated1 = 94;

		public const byte MurderSomeoneElsesPortal = 95;

		public const byte TeleportPlayerThroughPortal = 96;

		public const byte AchievementMessageNPCKilled = 97;

		public const byte AchievementMessageEventHappened = 98;

		public const byte MinionRestTargetUpdate = 99;

		public const byte TeleportNPCThroughPortal = 100;

		public const byte UpdateTowerShieldStrengths = 101;

		public const byte NebulaLevelupRequest = 102;

		public const byte MoonlordHorror = 103;

		public const byte ShopOverride = 104;

		public const byte GemLockToggle = 105;

		public const byte PoofOfSmoke = 106;

		public const byte SmartTextMessage = 107;

		public const byte WiredCannonShot = 108;

		public const byte MassWireOperation = 109;

		public const byte MassWireOperationPay = 110;

		public const byte ToggleParty = 111;

		public const byte SpecialFX = 112;

		public const byte CrystalInvasionStart = 113;

		public const byte CrystalInvasionWipeAllTheThingsss = 114;

		public const byte MinionAttackTargetUpdate = 115;

		public const byte CrystalInvasionSendWaitTime = 116;

		public const byte PlayerHurtV2 = 117;

		public const byte PlayerDeathV2 = 118;

		public const byte CombatTextString = 119;

		public const byte Emoji = 120;

		public const byte TEDisplayDollItemSync = 121;

		public const byte RequestTileEntityInteraction = 122;

		public const byte WeaponsRackTryPlacing = 123;

		public const byte TEHatRackItemSync = 124;

		public const byte SyncTilePicking = 125;

		public const byte SyncRevengeMarker = 126;

		public const byte RemoveRevengeMarker = 127;

		public const byte LandGolfBallInCup = 128;

		public const byte FinishedConnectingToServer = 129;

		public const byte FishOutNPC = 130;

		public const byte TamperWithNPC = 131;

		public const byte PlayLegacySound = 132;

		public const byte FoodPlatterTryPlacing = 133;

		public const byte UpdatePlayerLuckFactors = 134;

		public const byte DeadPlayer = 135;

		public const byte SyncCavernMonsterType = 136;

		public const byte RequestNPCBuffRemoval = 137;

		public const byte ClientSyncedInventory = 138;

		public const byte SetCountsAsHostForGameplay = 139;

		public const byte SetMiscEventValues = 140;

		public const byte RequestLucyPopup = 141;

		public const byte SyncProjectileTrackers = 142;

		public const byte Count = 143;
	}
}
