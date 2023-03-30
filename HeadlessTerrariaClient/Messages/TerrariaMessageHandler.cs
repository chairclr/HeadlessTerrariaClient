using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Messages;

internal class TerrariaMessageHandler
{
    private readonly HeadlessClient Client;

    private readonly BinaryReader Reader;

    private readonly MemoryStream InternalStream;

    public Dictionary<MessageType, HandleMessageMethod> MessageHandlerCache = new Dictionary<MessageType, HandleMessageMethod>();

    public TerrariaMessageHandler(HeadlessClient client)
    {
        Client = client;

        Reader = Client.TerrariaNetworkClient.Reader;

        InternalStream = (MemoryStream)Reader.BaseStream;

        foreach (MethodInfo method in client.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            HandleMessageAttribute? handleMessageAttribute = method.GetCustomAttribute<HandleMessageAttribute>();
            if (handleMessageAttribute is not null)
            {
                MessageHandlerCache.Add(handleMessageAttribute.MessageType, (HandleMessageMethod)method.CreateDelegate(typeof(HandleMessageMethod), Client));
            }
        }
    }

    public async Task ReceiveMessage(int start, int length)
    {
        InternalStream.Position = start;

        MessageType messageType = (MessageType)Reader.ReadByte();

        if (MessageHandlerCache.TryGetValue(messageType, out HandleMessageMethod? method))
        {
            await method(Reader);
        }
        else
        {
             Console.WriteLine($"Unhandled packet of type: {messageType} Length: {length}");
        }
    }
}

internal delegate Task HandleMessageMethod(BinaryReader reader);