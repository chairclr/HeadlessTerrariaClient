using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Terraria
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

        public void RandomizeAppearence()
        {
            // implement now 🤨 📸

            Random rnd = new Random();

            skinVariant = rnd.Next(11);
            hairType = rnd.Next(164);
            hairDye = rnd.Next(12);

            // Use the AARRGGBB system if you want here idk
            // too much thinking work for me im also lazy so cope 

            hairColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
            skinColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
            eyeColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
            shirtColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
            underShirtColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
            pantsColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
            shoeColor = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255));
        }
    }
}
