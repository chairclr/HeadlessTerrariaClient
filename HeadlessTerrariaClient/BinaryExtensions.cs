﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient;

public static class BinaryExtensions
{
    /// <summary>
    /// Writes a Color to a stream
    /// </summary>
    /// <param name="writer">stream to be written to</param>
    /// <param name="c">the Color to be written</param>
    public static void Write(this BinaryWriter writer, Color c)
    {
        writer.Write(c.R);
        writer.Write(c.G);
        writer.Write(c.B);
    }

    /// <summary>
    /// Reads a Color from a stream
    /// </summary>
    /// <param name="reader">stream to be read from</param>
    /// <returns>the Color read from the stream</returns>
    public static Color ReadRGB(this BinaryReader reader)
    {
        return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
    }

    /// <summary>
    /// Writes a Vector2 to a stream
    /// </summary>
    /// <param name="writer">stream to be written to</param>
    /// <param name="vec">the Vector2 to be written</param>
    public static void Write(this BinaryWriter writer, Vector2 vec)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
    }

    /// <summary>
    /// Reads a Vector2 from a stream
    /// </summary>
    /// <param name="reader">stream to be read from</param>
    /// <returns>the Vector2 read from the stream</returns>
    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
}
