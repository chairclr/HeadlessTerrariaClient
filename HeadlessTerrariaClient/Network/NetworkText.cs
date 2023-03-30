using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HeadlessTerrariaClient.Network;

public class NetworkText
{
    public static NetworkText Empty => new NetworkText("");

    private string Text;

    private NetworkTextMode Mode;

    private NetworkText[]? Substitutions;

    public NetworkText(string text)
        : this(text, NetworkTextMode.Literal)
    {

    }

    private NetworkText(string text, NetworkTextMode mode)
    {
        Text = text;
        Mode = mode;
    }

    private static NetworkText[] ConvertSubstitutionsToNetworkText(object[] substitutions)
    {
        NetworkText[] array = new NetworkText[substitutions.Length];
        for (int i = 0; i < substitutions.Length; i++)
        {
            NetworkText networkText = (substitutions![i]! as NetworkText)!;
            if (networkText == null)
            {
                networkText = FromLiteral(substitutions![i].ToString()!);
            }
            array[i] = networkText;
        }
        return array;
    }

    public static NetworkText FromFormattable(string text, params object[] substitutions)
    {
        return new NetworkText(text, NetworkTextMode.Formattable)
        {
            Substitutions = ConvertSubstitutionsToNetworkText(substitutions)
        };
    }

    public static NetworkText FromLiteral(string text)
    {
        return new NetworkText(text, NetworkTextMode.Literal);
    }

    public static NetworkText FromKey(string key, params object[] substitutions)
    {
        return new NetworkText(key, NetworkTextMode.LocalizationKey)
        {
            Substitutions = ConvertSubstitutionsToNetworkText(substitutions)
        };
    }

    public int GetMaxSerializedSize()
    {
        int num = 0;
        num++;
        num += 4 + Encoding.UTF8.GetByteCount(Text);
        if (Mode != 0)
        {
            num++;
            for (int i = 0; i < Substitutions!.Length; i++)
            {
                num += Substitutions[i].GetMaxSerializedSize();
            }
        }
        return num;
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)Mode);
        writer.Write(Text);
        SerializeSubstitutionList(writer);
    }

    private void SerializeSubstitutionList(BinaryWriter writer)
    {
        if (Mode != 0)
        {
            writer.Write((byte)Substitutions!.Length);
            for (int i = 0; i < (Substitutions.Length & 0xFF); i++)
            {
                Substitutions[i].Serialize(writer);
            }
        }
    }

    public static NetworkText Deserialize(BinaryReader reader)
    {
        NetworkTextMode mode = (NetworkTextMode)reader.ReadByte();
        NetworkText networkText = new NetworkText(reader.ReadString(), mode);
        networkText.DeserializeSubstitutionList(reader);
        return networkText;
    }

    public static NetworkText DeserializeLiteral(BinaryReader reader)
    {
        NetworkTextMode mode = (NetworkTextMode)reader.ReadByte();
        NetworkText networkText = new NetworkText(reader.ReadString(), mode);
        networkText.DeserializeSubstitutionList(reader);
        if (mode != 0)
        {
            networkText.SetToEmptyLiteral();
        }
        return networkText;
    }

    private void DeserializeSubstitutionList(BinaryReader reader)
    {
        if (Mode != 0)
        {
            Substitutions = new NetworkText[reader.ReadByte()];
            for (int i = 0; i < Substitutions.Length; i++)
            {
                Substitutions[i] = Deserialize(reader);
            }
        }
    }

    private void SetToEmptyLiteral()
    {
        Mode = NetworkTextMode.Literal;
        Text = string.Empty;
        Substitutions = null;
    }

    public override string ToString()
    {
        try
        {
            switch (Mode)
            {
                case NetworkTextMode.Literal:
                    return Text;
                case NetworkTextMode.Formattable:
                    {
                        string text2 = Text;
                        object[] substitutions = Substitutions!;
                        return string.Format(text2, substitutions!);
                    }
                //case Mode.LocalizationKey:
                //{
                //	string text = _text;
                //	object[] substitutions = _substitutions;
                //	return Language.GetTextValue(text, substitutions);
                //}
                default:
                    return Text;
            }
        }
        catch (Exception ex)
        {
            string.Concat(string.Concat("NetworkText.ToString() threw an exception.\n" + ToDebugInfoString(), "\n"), "Exception: ", ex?.ToString());
            SetToEmptyLiteral();
        }
        return Text;
    }

    private string ToDebugInfoString(string linePrefix = "")
    {
        string text = string.Format("{0}Mode: {1}\n{0}Text: {2}\n", linePrefix, Mode, Text);
        //if (_mode == Mode.LocalizationKey)
        //{
        //	text += $"{linePrefix}Localized Text: {Language.GetTextValue(_text)}\n";
        //}
        if (Mode != 0)
        {
            for (int i = 0; i < Substitutions!.Length; i++)
            {
                text += $"{linePrefix}Substitution {i}:\n";
                text += Substitutions[i].ToDebugInfoString(linePrefix + "\t");
            }
        }
        return text;
    }

    private enum NetworkTextMode : byte
    {
        Literal,
        Formattable,
        LocalizationKey
    }
}
