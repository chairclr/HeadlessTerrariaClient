using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HeadlessTerrariaClient.Utility;

namespace HeadlessTerrariaClient.Terraria.Chat
{
	public class ChatMessage
	{
		public int AuthorIndex;
		public string RawMessage;

        public ChatMessage(int a, string m)
        {
            AuthorIndex = a;
            RawMessage = m;
        }

		public void WriteToConsole()
        {
			List<TextSnippet> list = ChatParser.ParseMessage(RawMessage, Color.White);
			
			for (int i = 0; i < list.Count; i++)
            {
				TextSnippet current = list[i];
				ConsoleColor color = Util.PickNearbyConsoleColor(current.Color);

				Console.ForegroundColor = color;

				Console.Write(current.Text);

				Console.ResetColor();
            }
		}
		public string GetParsedText()
        {
			return ChatParser.ParseOutTags(RawMessage);
		}
		public override string ToString()
		{
			return GetParsedText();
		}
	}
    public class ChatParser
    {
		static ChatParser()
        {
			Register<Handlers.ColorTagHandler>(new string[2] { "c", "color" });
			Register<Handlers.ItemTagHandler>(new string[2] { "i", "item" });
			Register<Handlers.NameTagHandler>(new string[2] { "n", "name" });
		}

		public static readonly Regex ChatFormat = new Regex("(?<!\\\\)\\[(?<tag>[a-zA-Z]{1,10})(\\/(?<options>[^:]+))?:(?<text>.+?)(?<!\\\\)\\]", RegexOptions.Compiled);

		private static ConcurrentDictionary<string, ITagHandler> _handlers = new ConcurrentDictionary<string, ITagHandler>();

		public static void Register<T>(params string[] names) where T : ITagHandler, new()
		{
			T val = new T();
			for (int i = 0; i < names.Length; i++)
			{
				_handlers[names[i].ToLower()] = val;
			}
		}

		private static ITagHandler GetHandler(string tagName)
		{
			string key = tagName.ToLower();
			if (_handlers.ContainsKey(key))
			{
				return _handlers[key];
			}
			return null;
		}

		public static List<TextSnippet> ParseMessage(string text, Color baseColor)
		{
			text = text.Replace("\r", "");
			MatchCollection matchCollection = ChatFormat.Matches(text);
			List<TextSnippet> snippets = new List<TextSnippet>();
			int num = 0;
			foreach (Match item in matchCollection)
			{
				if (item.Index > num)
				{
					snippets.Add(new TextSnippet(text.Substring(num, item.Index - num), baseColor));
				}
				num = item.Index + item.Length;
				string tagValue = item.Groups["tag"].Value;
				string textValue = item.Groups["text"].Value;
				string optValue = item.Groups["options"].Value;
				ITagHandler tagHandler = GetHandler(tagValue);
				if (tagHandler != null)
				{
					snippets.Add(tagHandler.Parse(textValue, baseColor, optValue));
					snippets[snippets.Count - 1].TextOriginal = item.ToString();
				}
				else
				{
					snippets.Add(new TextSnippet(textValue, baseColor));
				}
			}
			if (text.Length > num)
			{
				snippets.Add(new TextSnippet(text.Substring(num, text.Length - num), baseColor));
			}
			return snippets;
		}

		public static string ParseOutTags(string text)
        {
			List<TextSnippet> list = ParseMessage(text, Color.White);

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
            {
				builder.Append(list[i].Text);
            }

			return builder.ToString();
		}

		public interface ITagHandler
		{
			TextSnippet Parse(string text, Color baseColor = default(Color), string options = null);
		}

		public class Handlers
        {
			public class ColorTagHandler : ITagHandler
			{
				TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
				{
					TextSnippet textSnippet = new TextSnippet(text);
					if (!int.TryParse(options, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
					{
						return textSnippet;
					}
					textSnippet.Color = new Color((result >> 16) & 0xFF, (result >> 8) & 0xFF, result & 0xFF);
					return textSnippet;
				}
			}

			public class ItemTagHandler : ITagHandler
			{
				private class ItemSnippet : TextSnippet
				{
					private Item _item;

					public ItemSnippet(Item item)
					{
						_item = item;
						Color = Color.White;
					}
				}

				TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
				{
					Item item = new Item();
					if (int.TryParse(text, out var result))
					{
						item.type = result;
					}
					if (item.type <= 0)
					{
						return new TextSnippet(text);
					}
					item.stack = 1;
					if (options != null)
					{
						string[] array = options.Split(',');
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i].Length == 0)
							{
								continue;
							}
							switch (array[i][0])
							{
								case 's':
								case 'x':
								{
									if (int.TryParse(array[i].Substring(1), out var result3))
									{
										item.stack = result3;
									}
									break;
								}
								case 'p':
								{
									if (int.TryParse(array[i].Substring(1), out var result2))
									{
										item.prefix = result2;
									}
									break;
								}
							}
						}
					}
					string text2 = "";
					if (item.stack > 1)
					{
						text2 = " (" + item.stack + ")";
					}
					return new ItemSnippet(item)
					{
						Text = "[" + text2 + "]",
						CheckForHover = true,
						DeleteWhole = true
					};
				}

				public static string GenerateTag(Item I)
				{
					string text = "[i";
					if (I.prefix != 0)
					{
						text = text + "/p" + I.prefix;
					}
					if (I.stack != 1)
					{
						text = text + "/s" + I.stack;
					}
					return text + ":" + I.type + "]";
				}
			}

			public class NameTagHandler : ITagHandler
			{
				TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
				{
					return new TextSnippet("<" + text.Replace("\\[", "[").Replace("\\]", "]") + ">", baseColor);
				}

				public static string GenerateTag(string name)
				{
					return "[n:" + name.Replace("[", "\\[").Replace("]", "\\]") + "]";
				}
			}

		}
	}
}
