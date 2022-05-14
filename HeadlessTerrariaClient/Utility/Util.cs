using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HeadlessTerrariaClient.Utility
{
    public static class Util
    {
		public static void WriteRGB(this BinaryWriter writer, Color c)
		{
			writer.Write(c.R);
			writer.Write(c.G);
			writer.Write(c.B);
		}
		public static Color ReadRGB(this BinaryReader reader)
		{
			return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
		}
		public static void Write(this BinaryWriter writer, Vector2 vec)
		{
			writer.Write(vec.X);
			writer.Write(vec.Y);
		}
		public static Vector2 ReadVector2(this BinaryReader reader)
        {
			return new Vector2(reader.ReadSingle(), reader.ReadSingle());
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

		public static double HueToRGB(double c, double t1, double t2)
		{
			if (c < 0.0)
			{
				c += 1.0;
			}
			if (c > 1.0)
			{
				c -= 1.0;
			}
			if (6.0 * c < 1.0)
			{
				return t1 + (t2 - t1) * 6.0 * c;
			}
			if (2.0 * c < 1.0)
			{
				return t2;
			}
			if (3.0 * c < 2.0)
			{
				return t1 + (t2 - t1) * (2.0 / 3.0 - c) * 6.0;
			}
			return t1;
		}
		public static Color HSLToRGB(float Hue, float Saturation, float Luminosity, byte a = byte.MaxValue)
		{
			byte r;
			byte g;
			byte b;
			if (Saturation == 0f)
			{
				r = (byte)Math.Round((double)Luminosity * 255.0);
				g = (byte)Math.Round((double)Luminosity * 255.0);
				b = (byte)Math.Round((double)Luminosity * 255.0);
			}
			else
			{
				double num = Hue;
				double num2 = ((!((double)Luminosity < 0.5)) ? ((double)(Luminosity + Saturation - Luminosity * Saturation)) : ((double)Luminosity * (1.0 + (double)Saturation)));
				double t = 2.0 * (double)Luminosity - num2;
				double c = num + 0.33333333333333331;
				double c2 = num;
				double c3 = num - 0.33333333333333331;
				c = HueToRGB(c, t, num2);
				c2 = HueToRGB(c2, t, num2);
				double num3 = HueToRGB(c3, t, num2);
				r = (byte)Math.Round(c * 255.0);
				g = (byte)Math.Round(c2 * 255.0);
				b = (byte)Math.Round(num3 * 255.0);
			}
			return new Color(a, r, g, b);
		}
		public static Color ScaledHslToRgb(Vector3 hsl)
		{
			return ScaledHslToRgb(hsl.X, hsl.Y, hsl.Z);
		}
		
		public static Color ScaledHslToRgb(float hue, float saturation, float luminosity)
		{
			return HSLToRGB(hue, saturation, luminosity * 0.85f + 0.15f);
		}

		public static Vector3 GetRandomColorVector()
        {
			return new Vector3(rand.NextFloat(), rand.NextFloat(), rand.NextFloat());
        }

		public static byte Clamp(byte n, byte min, byte max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static short Clamp(short n, short min, short max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static int Clamp(int n, int min, int max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static float Clamp(float n, float min, float max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static double Clamp(double n, double min, double max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		// borrowed from TShock btw
		private static readonly Dictionary<Color, ConsoleColor> consoleColorLookup = new Dictionary<Color, ConsoleColor>
		{
			{ Color.Red,                    ConsoleColor.Red },
			{ Color.Green,                  ConsoleColor.Green },
			{ Color.Blue,                   ConsoleColor.Cyan },
			{ new Color(255, 250, 170),     ConsoleColor.Yellow },
			{ new Color(170, 170, 255),     ConsoleColor.Cyan },
			{ new Color(255, 170, 255),     ConsoleColor.Magenta },
			{ new Color(170, 255, 170),     ConsoleColor.Green },
			{ new Color(255, 170, 170),     ConsoleColor.Red },
			{ new Color(139, 0, 0),         ConsoleColor.DarkRed },
			{ Color.PaleVioletRed,          ConsoleColor.Magenta },
			{ Color.White,                  ConsoleColor.White }
		};

		// borrowed from TShock btw
		public static ConsoleColor PickNearbyConsoleColor(Color color)
		{
			int ColorDiff(Color c1, Color c2)
			{
				return (int)((c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B));
			}

			var diffs = consoleColorLookup.Select(kvp => ColorDiff(kvp.Key, color));
			int index = 0;
			int min = int.MaxValue;

			for (int i = 0; i < consoleColorLookup.Count; i++)
			{
				if (diffs.ElementAt(i) < min)
				{
					index = i;
					min = diffs.ElementAt(i);
				}
			}

			return consoleColorLookup.Values.ElementAt(index);
		}


		public static ThreadSafeRandom rand = new ThreadSafeRandom();
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
