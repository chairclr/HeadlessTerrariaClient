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
        public Player[] player;

        /// <summary>
        /// World object containing information about the world
        /// </summary>
        public World CurrentWorld;

        public ClientWorld()
        {
            player = new Player[256];
            CurrentWorld = new World();

            for (int i = 0; i < 255; i++)
            {
                player[i] = new Player();
                player[i].whoAmI = i;
                player[i].active = false;
                player[i].name = "";
            }
        }

    }
}
