using System;
using System.Collections;
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

    [OutgoingMessage(MessageType.RequestWorldData)]
    private void WriteRequestWorldData()
    {

    }

    [OutgoingMessage(MessageType.SyncEquipment)]
    private void WriteSyncEquipment(int slot)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)slot);

        MessageWriter.Writer.Write((short)LocalPlayer.Inventory[slot].Stack);

        MessageWriter.Writer.Write((byte)LocalPlayer.Inventory[slot].Prefix);

        MessageWriter.Writer.Write((short)LocalPlayer.Inventory[slot].Type);
    }

    [OutgoingMessage(MessageType.SyncEquipment)]
    private void WriteSyncEquipment(int slot, Item item)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)slot);

        MessageWriter.Writer.Write((short)item.Stack);

        MessageWriter.Writer.Write((byte)item.Prefix);

        MessageWriter.Writer.Write((short)item.Type);
    }

    [OutgoingMessage(MessageType.SyncLoadout)]
    private void WriteSyncLoadout(int loadoutIndex = 0)
    {
        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        // Current loadout index
        MessageWriter.Writer.Write((byte)loadoutIndex);

        MessageWriter.Writer.WriteAccessoryVisibility(new bool[10]);
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
}
