using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Terraria
{
    public class RawPacket
    {
        public int MessageType;
        public BinaryReader Reader;
        public byte[] ReadBuffer;
        public bool ContinueWithPacket;
    }
}
