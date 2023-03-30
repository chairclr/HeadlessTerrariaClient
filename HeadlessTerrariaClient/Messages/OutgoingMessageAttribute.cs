using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Messages;

namespace HeadlessTerrariaClient;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal class OutgoingMessageAttribute : Attribute
{
    public MessageType MessageType;

    public OutgoingMessageAttribute(MessageType messageType)
    {
        MessageType = messageType;
    }
}
