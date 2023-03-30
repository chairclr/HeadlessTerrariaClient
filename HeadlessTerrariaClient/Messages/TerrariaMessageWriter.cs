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

        return InternalBuffer.AsMemory(0, length);
    }
}
