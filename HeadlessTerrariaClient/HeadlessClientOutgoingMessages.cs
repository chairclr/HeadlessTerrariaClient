using System;
using System.Collections;
using System.Numerics;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Messages;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient
{
    [OutgoingMessage(MessageType.Hello)]
    private void WriteHello(int version = 279)
    {
        MessageWriter.Writer.Write($"Terraria{version}");
    }

    [OutgoingMessage(MessageType.SyncPlayer)]
    private void WriteSyncLocalPlayer()
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((byte)LocalPlayer.Style.SkinVariant);

        MessageWriter.Writer.Write((byte)LocalPlayer.Style.HairType);

        MessageWriter.Writer.Write(LocalPlayer.Name);

        MessageWriter.Writer.Write((byte)LocalPlayer.Style.HairDye);

        MessageWriter.Writer.WriteAccessoryVisibility(new bool[10]);

        BitsByte hideMisc = 0;
        MessageWriter.Writer.Write(hideMisc);

        MessageWriter.Writer.Write(LocalPlayer.Style.HairColor);
        
        MessageWriter.Writer.Write(LocalPlayer.Style.SkinColor);
        
        MessageWriter.Writer.Write(LocalPlayer.Style.EyeColor);
        
        MessageWriter.Writer.Write(LocalPlayer.Style.ShirtColor);
        
        MessageWriter.Writer.Write(LocalPlayer.Style.UnderShirtColor);
        
        MessageWriter.Writer.Write(LocalPlayer.Style.PantsColor);
        
        MessageWriter.Writer.Write(LocalPlayer.Style.ShoeColor);
        
        BitsByte bites = (byte)0;

        if (LocalPlayer.Difficulty == PlayerDifficulty.Mediumcore)
        {
            bites[0] = true;
        }
        else if (LocalPlayer.Difficulty == PlayerDifficulty.Hardcode)
        {
            bites[1] = true;
        }
        else if (LocalPlayer.Difficulty == PlayerDifficulty.Creative)
        {
            bites[3] = true;
        }

        // extraAccessory
        bites[2] = false;
        
        MessageWriter.Writer.Write(bites);

        BitsByte torchAndSuperCart = 0;

        //torchAndSuperCart[0] = LocalPlayer.UsingBiomeTorches;
        //torchAndSuperCart[1] = LocalPlayer.happyFunTorchTime;
        //torchAndSuperCart[2] = LocalPlayer.unlockedBiomeTorches;
        //torchAndSuperCart[3] = LocalPlayer.unlockedSuperCart;
        //torchAndSuperCart[4] = LocalPlayer.enabledSuperCart;

        MessageWriter.Writer.Write(torchAndSuperCart);

        BitsByte miscBuffs = 0;

        //miscBuffs[0] = LocalPlayer.usedAegisCrystal;
        //miscBuffs[1] = LocalPlayer.usedAegisFruit;
        //miscBuffs[2] = LocalPlayer.usedArcaneCrystal;
        //miscBuffs[3] = LocalPlayer.usedGalaxyPearl;
        //miscBuffs[4] = LocalPlayer.usedGummyWorm;
        //miscBuffs[5] = LocalPlayer.usedAmbrosia;
        //miscBuffs[6] = LocalPlayer.ateArtisanBread;

        MessageWriter.Writer.Write(miscBuffs);
    }

    [OutgoingMessage(MessageType.SyncEquipment)]
    private void WriteSyncEquipment(int slot)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)slot);

        MessageWriter.Writer.Write((short)LocalPlayer.Inventory[slot].Stack);

        MessageWriter.Writer.Write((byte)LocalPlayer.Inventory[slot].Prefix);

        MessageWriter.Writer.Write((short)LocalPlayer.Inventory[slot].NetID);
    }

    [OutgoingMessage(MessageType.SyncEquipment)]
    private void WriteSyncEquipment(int slot, Item item)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)slot);

        MessageWriter.Writer.Write((short)item.Stack);

        MessageWriter.Writer.Write((byte)item.Prefix);

        MessageWriter.Writer.Write((short)item.NetID);
    }

    [OutgoingMessage(MessageType.SyncEquipment)]
    private void WriteSyncEquipment(int slot, int type, int stack, int prefix)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)slot);

        MessageWriter.Writer.Write((short)stack);

        MessageWriter.Writer.Write((byte)prefix);

        // Probably should move this to a different file
        int netId = type switch
        {
            3521 => -1,
            3520 => -2,
            3519 => -3,
            3518 => -4,
            3517 => -5,
            3516 => -6,
            3515 => -7,
            3514 => -8,
            3513 => -9,
            3512 => -10,
            3511 => -11,
            3510 => -12,
            3509 => -13,
            3508 => -14,
            3507 => -15,
            3506 => -16,
            3505 => -17,
            3504 => -18,
            3764 => -19,
            3765 => -20,
            3766 => -21,
            3767 => -22,
            3768 => -23,
            3769 => -24,
            3503 => -25,
            3502 => -26,
            3501 => -27,
            3500 => -28,
            3499 => -29,
            3498 => -30,
            3497 => -31,
            3496 => -32,
            3495 => -33,
            3494 => -34,
            3493 => -35,
            3492 => -36,
            3491 => -37,
            3490 => -38,
            3489 => -39,
            3488 => -40,
            3487 => -41,
            3486 => -42,
            3485 => -43,
            3484 => -44,
            3483 => -45,
            3482 => -46,
            3481 => -47,
            3480 => -48,
            _ => type,
        }; ;

        MessageWriter.Writer.Write((short)netId);
    }

    [OutgoingMessage(MessageType.PlayerControls)]
    private void WritePlayerControls()
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        BitsByte bitsByte25 = (byte)0;
        bitsByte25[0] = LocalPlayer.ControlUp;
        bitsByte25[1] = LocalPlayer.ControlDown;
        bitsByte25[2] = LocalPlayer.ControlLeft;
        bitsByte25[3] = LocalPlayer.ControlRight;
        bitsByte25[4] = LocalPlayer.ControlJump;
        bitsByte25[5] = LocalPlayer.ControlUseItem;
        bitsByte25[6] = LocalPlayer.Direction == 1;
        MessageWriter.Writer.Write(bitsByte25);

        BitsByte bitsByte26 = (byte)0;
        bitsByte26[0] = LocalPlayer.Pulley;
        bitsByte26[1] = LocalPlayer.Pulley && LocalPlayer.PulleyDir == 2;
        bitsByte26[2] = LocalPlayer.Velocity != Vector2.Zero;
        bitsByte26[3] = LocalPlayer.VortexStealthActive;
        bitsByte26[4] = LocalPlayer.GravDir == 1f;
        //bitsByte26[5] = LocalPlayer.ShieldRaised;
        bitsByte26[6] = LocalPlayer.Ghost;
        MessageWriter.Writer.Write(bitsByte26);

        BitsByte bitsByte27 = (byte)0;
        bitsByte27[0] = LocalPlayer.TryKeepingHoveringUp;
        bitsByte27[1] = LocalPlayer.IsVoidVaultEnabled;
        bitsByte27[2] = LocalPlayer.IsSitting;
        bitsByte27[3] = LocalPlayer.DownedDD2EventAnyDifficulty;
        bitsByte27[4] = LocalPlayer.IsPettingAnimal;
        bitsByte27[5] = LocalPlayer.IsTheAnimalBeingPetSmall;
        bitsByte27[6] = LocalPlayer.PotionOfReturnOriginalUsePosition.HasValue;
        bitsByte27[7] = LocalPlayer.TryKeepingHoveringDown;
        MessageWriter.Writer.Write(bitsByte27);

        BitsByte bitsByte28 = (byte)0;
        //bitsByte28[0] = LocalPlayer.Sleeping.isSleeping;
        //bitsByte28[1] = LocalPlayer.AutoReuseAllWeapons;
        //bitsByte28[2] = LocalPlayer.ControlDownHold;
        //bitsByte28[3] = LocalPlayer.IsOperatingAnotherEntity;
        //bitsByte28[4] = LocalPlayer.ControlUseTile;
        MessageWriter.Writer.Write(bitsByte28);

        MessageWriter.Writer.Write((byte)LocalPlayer.SelectedItem);
        MessageWriter.Writer.Write(LocalPlayer.Position);
        if (bitsByte26[2])
        {
            MessageWriter.Writer.Write(LocalPlayer.Velocity);
        }

        if (LocalPlayer.PotionOfReturnOriginalUsePosition.HasValue && LocalPlayer.PotionOfReturnHomePosition.HasValue)
        {
            MessageWriter.Writer.Write(LocalPlayer.PotionOfReturnOriginalUsePosition.Value);
            MessageWriter.Writer.Write(LocalPlayer.PotionOfReturnHomePosition.Value);
        }
    }

    [OutgoingMessage(MessageType.PlayerControls)]
    private void WriteSimplePlayerControls(int seletedItem, Vector2 position)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((byte)0);

        MessageWriter.Writer.Write((byte)0);

        MessageWriter.Writer.Write((byte)0);

        MessageWriter.Writer.Write((byte)0);

        MessageWriter.Writer.Write((byte)seletedItem);

        MessageWriter.Writer.Write(position);
    }

    [OutgoingMessage(MessageType.RequestWorldData)]
    private void WriteRequestWorldData()
    {

    }

    [OutgoingMessage(MessageType.SpawnTileData)]
    private void WriteSpawnTileData()
    {
        MessageWriter.Writer.Write(-1);

        MessageWriter.Writer.Write(-1);
    }

    [OutgoingMessage(MessageType.SpawnTileData)]
    private void WriteSpawnTileData(int spawnX, int spawnY)
    {
        MessageWriter.Writer.Write(spawnX);

        MessageWriter.Writer.Write(spawnY);
    }

    [OutgoingMessage(MessageType.PlayerSpawn)]
    private void WritePlayerSpawn(int context)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write(-1);
        MessageWriter.Writer.Write(-1);

        MessageWriter.Writer.Write(LocalPlayer.RespawnTimer);

        MessageWriter.Writer.Write(0);
        MessageWriter.Writer.Write(0);
        MessageWriter.Writer.Write((byte)context);
    }

    [OutgoingMessage(MessageType.PlayerLife)]
    private void WritePlayerLife()
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)LocalPlayer.Life);

        MessageWriter.Writer.Write((short)LocalPlayer.LifeMax);
    }

    [OutgoingMessage(MessageType.PlayerLife)]
    private void WritePlayerLife(int life, int? lifeMax = null)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)life);

        if (lifeMax.HasValue)
        {
            MessageWriter.Writer.Write((short)lifeMax.Value);
        }
        else
        {
            MessageWriter.Writer.Write((short)LocalPlayer.LifeMax);
        }
    }

    [OutgoingMessage(MessageType.PlayerMana)]
    private void WritePlayerMana()
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)LocalPlayer.Mana);

        MessageWriter.Writer.Write((short)LocalPlayer.ManaMax);
    }

    [OutgoingMessage(MessageType.PlayerMana)]
    private void WritePlayerMana(int mana, int? manaMax = null)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)mana);

        if (manaMax.HasValue)
        {
            MessageWriter.Writer.Write((short)manaMax.Value);
        }
        else
        {
            MessageWriter.Writer.Write((short)LocalPlayer.ManaMax);
        }
    }

    [OutgoingMessage(MessageType.PlayerBuffs)]
    private void WriteSyncPlayerBuffs()
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        for (int i = 0; i < 44; i++)
        {
            // buff type
            MessageWriter.Writer.Write((short)0);
        }
    }

    [OutgoingMessage(MessageType.ClientUUID)]
    private void WriteClientUUID()
    {
        MessageWriter.Writer.Write(ClientUUID);
    }

    [OutgoingMessage(MessageType.ClientUUID)]
    private void WriteClientUUID(string uuid)
    {
        MessageWriter.Writer.Write(uuid);
    }
    
    [OutgoingMessage(MessageType.SyncLoadout)]
    private void WriteSyncLoadout(int loadoutIndex = 0)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        // Current loadout index
        MessageWriter.Writer.Write((byte)loadoutIndex);

        MessageWriter.Writer.WriteAccessoryVisibility(new bool[10]);
    }
}
