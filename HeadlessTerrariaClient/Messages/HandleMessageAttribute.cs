using HeadlessTerrariaClient.Messages;

namespace HeadlessTerrariaClient;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal class HandleMessageAttribute : Attribute
{
    public MessageType MessageType;

    public HandleMessageAttribute(MessageType messageType)
    {
        MessageType = messageType;
    }
}
