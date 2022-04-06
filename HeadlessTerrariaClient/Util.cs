using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Text;

namespace HeadlessTerrariaClient
{
    public static class Util
    {
		public static void WriteRGB(this BinaryWriter bb, Color c)
		{
			bb.Write(c.R);
			bb.Write(c.G);
			bb.Write(c.B);
		}

		public static Color ReadRGB(this BinaryReader bb)
		{
			return Color.FromArgb(bb.ReadByte(), bb.ReadByte(), bb.ReadByte());
		}


		public static uint ComputeByteHash(byte[] s, int len)
		{
			uint num = default(uint);
			if (s != null)
			{
				num = 2166136261u;
				for (int i = 0; i < len; i++)
				{
					num = (s[i] ^ num) * 16777619;
				}
			}
			return num;
		}
		public static object GetDefault(Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}


	}
	public class ChatMessage
	{
		public int author;
		public string message;

		public ChatMessage(int a, string m)
		{
			author = a;
			message = m;
		}
	}

	public class PlayerDifficultyID
	{
		public const byte SoftCore = 0;

		public const byte MediumCore = 1;

		public const byte Hardcore = 2;

		public const byte Creative = 3;
	}
	class Settings : DynamicObject
	{
		private Dictionary<string, object> SettingValues = new Dictionary<string, object>();
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			string name = binder.Name;

			if (SettingValues.ContainsKey(name))
            {
				result = SettingValues[name];
			}
			else
            {
				result = Util.GetDefault(binder.ReturnType);
            }


			return true;
		}
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			string name = binder.Name;

			if (SettingValues.ContainsKey(name))
			{
				if (value.GetType() == SettingValues[name].GetType())
				{
					SettingValues[name] = value;
				}
				else
                {
					throw new ArgumentException($"Type missmatch, expected '{SettingValues[name].GetType().FullName}' got '{value.GetType().FullName}'");

				}
			}
			else
			{
				SettingValues.Add(name, value);
			}

			return true;
		}

	}
}
