using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using HeadlessTerrariaClient;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Util;

namespace HeadlessTerrariaClient.Terraria
{
    public class Player
    {
        public int whoAmI;
        public bool active = false;
        public string name = "";

        public int skinVariant;
        public int hairType;
        public int hairDye;
        public int difficulty;
        public bool extraAccessory;


        public Color hairColor;
        public Color skinColor;
        public Color eyeColor;
        public Color shirtColor;
        public Color underShirtColor;
        public Color pantsColor;
        public Color shoeColor;

        public int statLife;
        public int statLifeMax;
        public int statMana;
        public int statManaMax;

        public Vector2 position;
        public Vector2 velocity;

        public void SyncDataWithTemp(PlayerData data)
        {
            name = data.name;

            skinVariant = data.skinVariant;
            hairType = data.hairType;
            hairDye = data.hairDye;

            hairColor = data.hairColor;
            skinColor = data.skinColor;
            eyeColor = data.eyeColor;
            shirtColor = data.shirtColor;
            underShirtColor = data.underShirtColor;
            pantsColor = data.pantsColor;
            shoeColor = data.shoeColor;
            statLife = data.statLife;
            statLifeMax = data.statLifeMax;
            statMana = data.statMana;
            statManaMax = data.statManaMax;
        }

        public void LoadDefaultAppearence()
        {
            skinVariant = 0;
            hairType = 0;
            hairDye = 0;
            hairColor = new Color(215, 90, 55, 255);
            skinColor = new Color(255, 125, 90, 255);
            eyeColor = new Color(105, 90, 75, 255);
            shirtColor = new Color(175, 165, 140, 255);
            underShirtColor = new Color(160, 180, 215, 255);
            pantsColor = new Color(255, 230, 175, 255);
            shoeColor = new Color(160, 105, 60, 255);
        }
        public void RandomizeAppearence()
        {
            HairStyles.Rebuild();
            hairType = HairStyles.GetRandomHair();
            skinVariant = PlayerVariantID.GetRandomSkin();

            eyeColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            while (eyeColor.R + eyeColor.G + eyeColor.B > 300)
            {
                eyeColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            }
            float num = (float)ClientUtil.rand.Next(60, 120) * 0.01f;
            if (num > 1f)
            {
                num = 1f;
            }
            skinColor.R = (byte)((float)ClientUtil.rand.Next(240, 255) * num);
            skinColor.G = (byte)((float)ClientUtil.rand.Next(110, 140) * num);
            skinColor.B = (byte)((float)ClientUtil.rand.Next(75, 110) * num);
            hairColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            shirtColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            underShirtColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            pantsColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            shoeColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
        }
    }

    public class PlayerData
    {
        public string name = "";

        public int skinVariant = 0;
        public int hairType = 0;
        public int hairDye = 0;
        public int difficulty = 0;
        public bool extraAccessory = false;

        public Color hairColor = Color.White;
        public Color skinColor = Color.White;
        public Color eyeColor = Color.White;
        public Color shirtColor = Color.White;
        public Color underShirtColor = Color.White;
        public Color pantsColor = Color.White;
        public Color shoeColor = Color.White;

        public int statLife = 100;
        public int statLifeMax = 100;

        public int statMana = 20;
        public int statManaMax = 20;

        public void LoadDefaultAppearence()
        {
            skinVariant = 0;
            hairType = 0;
            hairDye = 0;
            hairColor = new Color(215, 90, 55, 255);
            skinColor = new Color(255, 125, 90, 255);
            eyeColor = new Color(105, 90, 75, 255);
            shirtColor = new Color(175 ,165 ,140 ,255);
            underShirtColor = new Color(160, 180, 215, 255);
            pantsColor = new Color(255 ,230 ,175, 255);
            shoeColor = new Color(160, 105, 60, 255);
        }
        public void RandomizeAppearence()
        {
            // implemented cringeface 🤨 📸
            HairStyles.Rebuild();
            hairType = HairStyles.GetRandomHair();
            skinVariant = PlayerVariantID.GetRandomSkin();

            eyeColor = Util.ClientUtil.ScaledHslToRgb(Util.ClientUtil.GetRandomColorVector());
            while (eyeColor.R + eyeColor.G + eyeColor.B > 300)
            {
                eyeColor = Util.ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            }
            float num = (float)Util.ClientUtil.rand.Next(60, 120) * 0.01f;
            if (num > 1f)
            {
                num = 1f;
            }
            skinColor.R = (byte)((float)ClientUtil.rand.Next(240, 255) * num);
            skinColor.G = (byte)((float)ClientUtil.rand.Next(110, 140) * num);
            skinColor.B = (byte)((float)ClientUtil.rand.Next(75, 110) * num);
            hairColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            shirtColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            underShirtColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            pantsColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
            shoeColor = ClientUtil.ScaledHslToRgb(ClientUtil.GetRandomColorVector());
        }
    }
}
