using System;
using System.Collections.Generic;
using HeadlessTerrariaClient.Utility;
using System.Text;

namespace HeadlessTerrariaClient.Terraria
{
	public class TextSnippet
	{
		public string Text;

		public string TextOriginal;

		public Color Color = Color.White;

		public float Scale = 1f;

		public bool CheckForHover;

		public bool DeleteWhole;

		public TextSnippet(string text = "")
		{
			Text = text;
			TextOriginal = text;
		}

		public TextSnippet(string text, Color color, float scale = 1f)
		{
			Text = text;
			TextOriginal = text;
			Color = color;
			Scale = scale;
		}

		public virtual TextSnippet CopyMorph(string newText)
		{
			TextSnippet obj = (TextSnippet)MemberwiseClone();
			obj.Text = newText;
			return obj;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
