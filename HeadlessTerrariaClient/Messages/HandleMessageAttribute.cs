using HeadlessTerrariaClient.Messages;

namespace HeadlessTerrariaClient;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal class IncomingMessageAttribute : Attribute
{
    public MessageType MessageType;

    public IncomingMessageAttribute(MessageType messageType)
    {
        MessageType = messageType;
    }
}
