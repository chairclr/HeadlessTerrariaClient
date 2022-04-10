using System;
using System.Collections.Generic;
using System.Text;

namespace HeadlessTerrariaClient.Terraria.Chat
{
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
}
