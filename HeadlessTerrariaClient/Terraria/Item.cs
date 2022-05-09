using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Utility;
using System.Net.Sockets;
using System.Numerics;

namespace HeadlessTerrariaClient.Terraria
{
    public class Item
    {
        public int type;
        public int stack;
        public int prefix;

        public Item()
        {
            type = 0;
            stack = 0;
            prefix = 0;
        }
        public Item(int type, int stack = 1, int prefix = 0)
        {
            this.type = type;
            this.stack = stack;
            this.prefix = prefix;
        }

        public Item Clone()
        {
            return new Item(type, stack, prefix);
        }
    }
}
