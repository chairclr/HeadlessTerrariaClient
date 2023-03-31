using System.Reflection;
using System.Text;

namespace HeadlessTerrariaClient.Messages;

internal partial class TerrariaMessageHandler
{
    private readonly HeadlessClient Client;

    private readonly BinaryReader Reader;

    private readonly MemoryStream InternalStream;

    internal int LastPacketLength = 0;

    public TerrariaMessageHandler(HeadlessClient client)
    {
        Client = client;

        Reader = Client.TCPNetworkClient.Reader;

        InternalStream = (MemoryStream)Reader.BaseStream;
    }

    public async void ReceiveMessage(int start, int length)
    {
        InternalStream.Position = start;

        MessageType messageType = (MessageType)Reader.ReadByte();

        LastPacketLength = length;

#if DEBUG
        byte[] internalBuffer = InternalStream.GetBuffer();

        StringBuilder builder = new StringBuilder();

        builder.Append($"T: {(int)messageType,3} L: {length + 2,5} B: {{ ");

        for (int i = start - 2; i < start + length; i++)
        {
            builder.Append(internalBuffer[i]);

            if (i + 1 < start + length)
                builder.Append(", ");
        }

        builder.Append(" }");

        Console.WriteLine($"↓ {builder.ToString()}");
#endif

        await HandleIncomingMessageAsync(messageType, Reader);
        HandleIncomingMessage(messageType, Reader);
    }
}