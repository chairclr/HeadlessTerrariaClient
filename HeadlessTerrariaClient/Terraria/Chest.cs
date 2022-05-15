using System;
using System.Collections.Generic;
using System.Text;

namespace HeadlessTerrariaClient.Terraria
{
    public class Chest
    {
		public Item[] Items;

		public string Name;

		public int x;

		public int y;

		public bool bankChest;


		public int frameCounter;

		public int frame;

		public Chest(bool bank = false)
		{
			Items = new Item[40];
			bankChest = bank;
			Name = string.Empty;
		}

		public override string ToString()
		{
			int num = 0;
			for (int i = 0; i < Items.Length; i++)
			{
				if (Items[i].stack > 0)
				{
					num++;
				}
			}
			return $"{{X: {x}, Y: {y}, Count: {num}}}";
		}
	}
}
