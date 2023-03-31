using System.Text;

namespace HeadlessTerrariaClient.Messages;

public class TerrariaMessageWriter
{
    public readonly BinaryWriter Writer;

    private readonly MemoryStream InternalStream;

    private readonly byte[] InternalBuffer;

    public TerrariaMessageWriter(BinaryWriter writer)
    {
        Writer = writer;

        InternalStream = (MemoryStream)Writer.BaseStream;

        InternalBuffer = InternalStream.GetBuffer();
    }

    public void BeginMessage(MessageType type)
    {
        InternalStream.Position = 2;

        Writer.Write((byte)type);
    }

    public ReadOnlyMemory<byte> EndMessage()
    {
        ushort length = (ushort)InternalStream.Position;

        InternalStream.Position = 0;

        Writer.Write(length);

#if DEBUG
        byte[] internalBuffer = InternalStream.GetBuffer();

        StringBuilder builder = new StringBuilder();

        builder.Append($"T: {(int)internalBuffer[2],3} L: {length,5} B: {{ ");

        for (int i = 0; i < length; i++)
        {
            builder.Append(internalBuffer[i]);

            if (i + 1 < length)
                builder.Append(", ");
        }

        builder.Append(" }");

        Console.WriteLine($"↑ {builder.ToString()}");
#endif

        return InternalBuffer.AsMemory(0, length);
    }
}
