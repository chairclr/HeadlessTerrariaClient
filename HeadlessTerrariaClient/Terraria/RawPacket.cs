using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HeadlessTerrariaClient.Terraria
{
    public class RawPacket
    {
        public int MessageType;
        public bool ContinueWithPacket;
    }
    public class RawIncomingPacket : RawPacket
    {
        public byte[] ReadBuffer;
        public BinaryReader Reader;
    }
    public class RawOutgoingPacket : RawPacket
    {
        public byte[] WriteBuffer;
        public BinaryWriter Writer;
    }
}
