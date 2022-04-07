using System.Collections.Generic;

namespace Terraria.ID
{
	public static class PlayerVariantID
	{
		public const int MaleStarter = 0;

		public const int MaleSticker = 1;

		public const int MaleGangster = 2;

		public const int MaleCoat = 3;

		public const int FemaleStarter = 4;

		public const int FemaleSticker = 5;

		public const int FemaleGangster = 6;

		public const int FemaleCoat = 7;

		public const int MaleDress = 8;

		public const int FemaleDress = 9;

		public const int MaleDisplayDoll = 10;

		public const int FemaleDisplayDoll = 11;

		public const int Count = 12;

		public static int GetRandomSkin()
		{
			return HeadlessTerrariaClient.Util.rand.Next(Count);
		}
	}
	public static class HairStyles
	{
		public static List<int> AvailableHairstyles = new List<int>(256);
		public static void Rebuild(bool _isAtCharacterCreation = true, bool _isAtStylist = false, bool _defeatedPlantera = false, bool _defeatedMartians = false, bool _defeatedMoonlord = false)
        {
			List<int> availableHairstyles = AvailableHairstyles;
			availableHairstyles.Clear();
			if (_isAtCharacterCreation || _isAtStylist)
			{
				for (int i = 0; i < 51; i++)
				{
					availableHairstyles.Add(i);
				}
				availableHairstyles.Add(136);
				availableHairstyles.Add(137);
				availableHairstyles.Add(138);
				availableHairstyles.Add(139);
				availableHairstyles.Add(140);
				availableHairstyles.Add(141);
				availableHairstyles.Add(142);
				availableHairstyles.Add(143);
				availableHairstyles.Add(144);
				availableHairstyles.Add(147);
				availableHairstyles.Add(148);
				availableHairstyles.Add(149);
				availableHairstyles.Add(150);
				availableHairstyles.Add(151);
				availableHairstyles.Add(154);
				availableHairstyles.Add(155);
				availableHairstyles.Add(157);
				availableHairstyles.Add(158);
				availableHairstyles.Add(161);
			}
			if (_isAtStylist)
			{
				for (int j = 51; j < 123; j++)
				{
					availableHairstyles.Add(j);
				}
				availableHairstyles.Add(134);
				availableHairstyles.Add(135);
				availableHairstyles.Add(145);
				availableHairstyles.Add(146);
				availableHairstyles.Add(152);
				availableHairstyles.Add(153);
				availableHairstyles.Add(156);
				availableHairstyles.Add(159);
				availableHairstyles.Add(160);
				availableHairstyles.Add(163);
				availableHairstyles.Add(164);
				if (_defeatedPlantera)
				{
					availableHairstyles.Add(162);
				}
				if (_defeatedMartians)
				{
					availableHairstyles.AddRange(new int[10] { 132, 131, 130, 129, 128, 127, 126, 125, 124, 123 });
				}
				if (_defeatedMartians && _defeatedMoonlord)
				{
					availableHairstyles.Add(133);
				}
			}
		}
		public static int GetRandomHair()
        {
			return AvailableHairstyles[HeadlessTerrariaClient.Util.rand.Next(AvailableHairstyles.Count)];
        }
	}
}
