using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using HeadlessTerrariaClient;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Utility;

namespace HeadlessTerrariaClient.Terraria
{
    /// <summary>
    /// A player in the world
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Index in World.player
        /// </summary>
        public int whoAmI;

        /// <summary>
        /// Whether or not a player is active
        /// </summary>
        public bool active = false;

        /// <summary>
        /// Name of the player
        /// </summary>
        public string name = "";

        /// <summary>
        /// ayo is this client racist?
        /// </summary>
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

        /// <summary>
        /// The inventory of the player
        /// </summary>
        public Item[] inventory = new Item[260];


        /// <summary>
        /// Position of the player
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// Velocity of the player
        /// </summary>
        public Vector2 velocity;

        /// <summary>
        /// Loads the default appearnce of the player
        /// </summary>
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

        /// <summary>
        /// Randomizes the appearence of the player
        /// </summary>
        public void RandomizeAppearence()
        {
            // implemented cringeface 🤨 📸
            HairStyles.Rebuild();
            hairType = HairStyles.GetRandomHair();
            skinVariant = PlayerVariantID.GetRandomSkin();

            eyeColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
            while (eyeColor.R + eyeColor.G + eyeColor.B > 300)
            {
                eyeColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
            }
            float num = (float)Util.rand.Next(60, 120) * 0.01f;
            if (num > 1f)
            {
                num = 1f;
            }
            skinColor.R = (byte)((float)Util.rand.Next(240, 255) * num);
            skinColor.G = (byte)((float)Util.rand.Next(110, 140) * num);
            skinColor.B = (byte)((float)Util.rand.Next(75, 110) * num);
            hairColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
            shirtColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
            underShirtColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
            pantsColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
            shoeColor = Util.ScaledHslToRgb(Util.GetRandomColorVector());
        }

        /// <summary>
        /// Loads the default inventory of the player
        /// </summary>
        public void LoadDefaultInventory()
        {
            inventory[0] = new Item(ItemID.CopperShortsword);
            inventory[1] = new Item(ItemID.CopperPickaxe);
            inventory[2] = new Item(ItemID.CopperAxe);
        }

        public void Reset()
        {
            ResetInventory();
            name = "";
            active = false;
        }

        public void ResetInventory()
        {
            inventory = new Item[260];
            for (int i = 0; i < inventory.Length; i++)
            {
                inventory[i] = new Item();
            }
        }

        /// <summary>
        /// Loads both the default inventory and appearnce of the player
        /// </summary>
        public void LoadDefaultPlayer()
        {
            difficulty = PlayerDifficultyID.SoftCore;
            LoadDefaultAppearence();
            LoadDefaultInventory();
        }
    }
}
