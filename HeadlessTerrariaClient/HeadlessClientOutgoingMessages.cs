using System;
using System.Collections;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Messages;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient
{
    [OutgoingMessage]
    private void WriteHello(int version = 279)
    {
        MessageWriter.BeginMessage(MessageType.Hello);

        MessageWriter.Writer.Write($"Terraria{version}");
    }

    [OutgoingMessage]
    private void WriteSyncLocalPlayer()
    {
        MessageWriter.BeginMessage(MessageType.SyncPlayer);

        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write(LocalPlayer.Style.SkinVariant);

        MessageWriter.Writer.Write(LocalPlayer.Style.HairType);

        MessageWriter.Writer.Write(LocalPlayer.Name);

        MessageWriter.Writer.Write(LocalPlayer.Style.HairDye);

        BitsByte hideVisibleAccessory = 0;
        MessageWriter.Writer.Write(hideVisibleAccessory);

        BitsByte hideVisibleAccessory2 = 0;
        MessageWriter.Writer.Write(hideVisibleAccessory2);

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

        // ?? no idea
        bites[2] = false;

        MessageWriter.Writer.Write(bites);

        BitsByte bitsByte8 = 0;

        //UsingBiomeTorches;
        bitsByte8[0] = false;

        //happyFunTorchTime;
        bitsByte8[1] = false;

        //unlockedBiomeTorches;
        bitsByte8[2] = false;

        MessageWriter.Writer.Write(bitsByte8);
    }

    [OutgoingMessage]
    private void WriteClientUUID()
    {
        MessageWriter.BeginMessage(MessageType.ClientUUID);

        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write(ClientUUID);
    }

    [OutgoingMessage]
    private void WritePlayerLife()
    {
        MessageWriter.BeginMessage(MessageType.PlayerLife);

        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)LocalPlayer.Life);

        MessageWriter.Writer.Write((short)LocalPlayer.LifeMax);
    }

    [OutgoingMessage]
    private void WritePlayerMana()
    {
        MessageWriter.BeginMessage(MessageType.PlayerMana);

        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)LocalPlayer.Mana);

        MessageWriter.Writer.Write((short)LocalPlayer.ManaMax);
    }

    [OutgoingMessage]
    private void WriteSyncPlayerBuffs()
    {
        MessageWriter.BeginMessage(MessageType.PlayerBuffs);

        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        for (int i = 0; i < 44; i++)
        {
            // buff type
            MessageWriter.Writer.Write((short)0);
        }
    }

    [OutgoingMessage]
    private void WriteRequestWorldData()
    {
        MessageWriter.BeginMessage(MessageType.RequestWorldData);
    }

    [OutgoingMessage]
    private void WriteSyncEquipment(int slot)
    {
        MessageWriter.BeginMessage(MessageType.SyncEquipment);

        MessageWriter.Writer.Write((byte)LocalPlayerIndex);

        MessageWriter.Writer.Write((short)slot);

        MessageWriter.Writer.Write((short)LocalPlayer.Inventory[slot].Stack);

        MessageWriter.Writer.Write((byte)LocalPlayer.Inventory[slot].Prefix);

        MessageWriter.Writer.Write((short)LocalPlayer.Inventory[slot].Type);
    }
}
