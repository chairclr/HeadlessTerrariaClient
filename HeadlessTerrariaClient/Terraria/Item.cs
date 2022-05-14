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

        public bool active;

		public Vector2 position;
		public Vector2 velocity;

		public bool instanced;
		public int whoIsThisInstancedItemFor;

        public Item()
        {
            type = 0;
            stack = 0;
            prefix = 0;
            this.active = false;
        }
        public Item(int type, int stack = 1, int prefix = 0, bool active = false)
        {
            this.type = type;
            this.stack = stack;
            this.prefix = prefix;
            this.active = active;
        }

        public void SetTypeFromNetID(int netID)
        {
			if (netID < 0)
			{
				switch (netID)
				{
					case -1:
						SetType(3521);
						break;
					case -2:
						SetType(3520);
						break;
					case -3:
						SetType(3519);
						break;
					case -4:
						SetType(3518);
						break;
					case -5:
						SetType(3517);
						break;
					case -6:
						SetType(3516);
						break;
					case -7:
						SetType(3515);
						break;
					case -8:
						SetType(3514);
						break;
					case -9:
						SetType(3513);
						break;
					case -10:
						SetType(3512);
						break;
					case -11:
						SetType(3511);
						break;
					case -12:
						SetType(3510);
						break;
					case -13:
						SetType(3509);
						break;
					case -14:
						SetType(3508);
						break;
					case -15:
						SetType(3507);
						break;
					case -16:
						SetType(3506);
						break;
					case -17:
						SetType(3505);
						break;
					case -18:
						SetType(3504);
						break;
					case -19:
						SetType(3764);
						break;
					case -20:
						SetType(3765);
						break;
					case -21:
						SetType(3766);
						break;
					case -22:
						SetType(3767);
						break;
					case -23:
						SetType(3768);
						break;
					case -24:
						SetType(3769);
						break;
					case -25:
						SetType(3503);
						break;
					case -26:
						SetType(3502);
						break;
					case -27:
						SetType(3501);
						break;
					case -28:
						SetType(3500);
						break;
					case -29:
						SetType(3499);
						break;
					case -30:
						SetType(3498);
						break;
					case -31:
						SetType(3497);
						break;
					case -32:
						SetType(3496);
						break;
					case -33:
						SetType(3495);
						break;
					case -34:
						SetType(3494);
						break;
					case -35:
						SetType(3493);
						break;
					case -36:
						SetType(3492);
						break;
					case -37:
						SetType(3491);
						break;
					case -38:
						SetType(3490);
						break;
					case -39:
						SetType(3489);
						break;
					case -40:
						SetType(3488);
						break;
					case -41:
						SetType(3487);
						break;
					case -42:
						SetType(3486);
						break;
					case -43:
						SetType(3485);
						break;
					case -44:
						SetType(3484);
						break;
					case -45:
						SetType(3483);
						break;
					case -46:
						SetType(3482);
						break;
					case -47:
						SetType(3481);
						break;
					case -48:
						SetType(3480);
						break;
				}
			}
			else
			{
				SetType(netID);
			}
		}

		public void SetType(int type)
        {
			this.type = type;
        }

		public int GetNetID()
		{
			switch (this.type)
			{
				case 3521:
					return -1;
				case 3520:
					return -2;
				case 3519:
					return -3;
				case 3518:
					return -4;
				case 3517:
					return -5;
				case 3516:
					return -6;
				case 3515:
					return -7;
				case 3514:
					return -8;
				case 3513:
					return -9;
				case 3512:
					return -10;
				case 3511:
					return -11;
				case 3510:
					return -12;
				case 3509:
					return -13;
				case 3508:
					return -14;
				case 3507:
					return -15;
				case 3506:
					return -16;
				case 3505:
					return -17;
				case 3504:
					return -18;
				case 3764:
					return -19;
				case 3765:
					return -20;
				case 3766:
					return -21;
				case 3767:
					return -22;
				case 3768:
					return -23;
				case 3769:
					return -24;
				case 3503:
					return -25;
				case 3502:
					return -26;
				case 3501:
					return -27;
				case 3500:
					return -28;
				case 3499:
					return -29;
				case 3498:
					return -30;
				case 3497:
					return -31;
				case 3496:
					return -32;
				case 3495:
					return -33;
				case 3494:
					return -34;
				case 3493:
					return -35;
				case 3492:
					return -36;
				case 3491:
					return -37;
				case 3490:
					return -38;
				case 3489:
					return -39;
				case 3488:
					return -40;
				case 3487:
					return -41;
				case 3486:
					return -42;
				case 3485:
					return -43;
				case 3484:
					return -44;
				case 3483:
					return -45;
				case 3482:
					return -46;
				case 3481:
					return -47;
				case 3480:
					return -48;

			}
			return this.type;
		}

		/// <summary>
		/// Creates a copy of an item
		/// </summary>
		/// <returns>Copy of the item</returns>
		public Item Clone()
		{
			Item i = new Item(type, stack, prefix, active);
			i.position = position;
			i.velocity = velocity;
			return i;
		}
    }
}
