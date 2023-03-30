using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient;

[AttributeUsage(AttributeTargets.Method)]
public class OutgoingMessageAttribute : Attribute
{

}
