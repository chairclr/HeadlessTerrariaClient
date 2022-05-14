using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using System.Net.Sockets;
using System.Numerics;

namespace HeadlessTerrariaClient.Client
{
    /// <summary>
    /// A world 
    /// </summary>
    public class ClientWorld
    {
        /// <summary>
        /// Players in the world
        /// </summary>
        public Player[] Players { get; set; }

        /// <summary>
        /// World object containing information about the world
        /// </summary>
        public World CurrentWorld;

        public Item[] Items { get; set; }

        public ClientWorld()
        {
            InitializePlayers();
            InitializeWorld();
        }

        private void InitializePlayers()
        {
            Players = new Player[256];
            for (int i = 0; i < 255; i++)
            {
                Players[i] = new Player();
                Players[i].Reset();
                Players[i].whoAmI = i;
            }

            Items = new Item[401];
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new Item();
            }
        }
        private void InitializeWorld()
        {
            CurrentWorld = new World();
        }
    }
}
