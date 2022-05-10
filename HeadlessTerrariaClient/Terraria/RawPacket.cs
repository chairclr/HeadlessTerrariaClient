using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HeadlessTerrariaClient.Terraria
{
    /// <summary>
    /// A raw packet
    /// </summary>
    public class RawPacket
    {
        /// <summary>
        /// Message type of the packet
        /// </summary>
        public int MessageType;

        /// <summary>
        /// Set by the callback
        /// If set to false, will ignore the packet
        /// </summary>
        public bool ContinueWithPacket;
    }

    /// <summary>
    /// Raw packet incoming from the server
    /// </summary>
    public class RawIncomingPacket : RawPacket
    {
        public byte[] ReadBuffer;
        public BinaryReader Reader;
    }

    /// <summary>
    /// Raw packet outgoing from the client
    /// </summary>
    public class RawOutgoingPacket : RawPacket
    {
        public byte[] WriteBuffer;
        public BinaryWriter Writer;
    }
}
