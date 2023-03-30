using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Game;

public class Player
{
    public int Index;

    public bool Active;

    public string Name = "<unnamed player>";

    public PlayerStyle Style = PlayerStyle.Default;

    public int Life;

    public int LifeMax;

    public int Mana;

    public int ManaMax;

    public Vector2 Position;

    public Vector2 Velocity;

    public Vector2? PotionOfReturnOriginalUsePosition;

    public Vector2? PotionOfReturnHomePosition;

    public PlayerDifficulty Difficulty;

    public bool IsVoidVaultEnabled;

    public bool DownedDD2EventAnyDifficulty;

    public bool IsPettingAnimal;

    public bool IsTheAnimalBeingPetSmall;

    public bool IsSitting;

    public bool ControlUp;
    
    public bool ControlDown;
    
    public bool ControlLeft;
    
    public bool ControlRight;
    
    public bool ControlJump;
    
    public bool TryKeepingHoveringDown;
    
    public bool TryKeepingHoveringUp;
    
    public bool ControlUseItem;

    public Item[] Inventory = new Item[350];

    public int SelectedItem;

    public bool Pulley;
    
    public bool VortexStealthActive;
    
    public bool Ghost;

    public byte PulleyDir;

    public int Direction;

    public int GravDir;

    public class PlayerStyle
    {
        public int SkinVariant;

        public int HairType;

        public int HairDye;

        public int Difficulty;

        public bool ExtraAccessory;

        public Color HairColor;

        public Color SkinColor;

        public Color EyeColor;

        public Color ShirtColor;

        public Color UnderShirtColor;

        public Color PantsColor;

        public Color ShoeColor;

        public static PlayerStyle Default => new PlayerStyle()
        {
            SkinVariant = 0,
            HairType = 0,
            HairDye = 0,
            HairColor = new Color(215, 90, 55, 255),
            SkinColor = new Color(255, 125, 90, 255),
            EyeColor = new Color(105, 90, 75, 255),
            ShirtColor = new Color(175, 165, 140, 255),
            UnderShirtColor = new Color(160, 180, 215, 255),
            PantsColor = new Color(255, 230, 175, 255),
            ShoeColor = new Color(160, 105, 60, 255)
        };
    }

    public Player()
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            Inventory[i] = new Item();
        }
    }
}
