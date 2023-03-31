using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessTerrariaClient.Messages;

public enum NetModuleType
{
    Liquid = 0,
    Text = 1,
    Ping = 2,
    Ambience = 3,
    Bestiary = 4,
    CreativeUnlocks = 5,
    CreativePowers = 6,
    CreativeUnlocksPlayerReport = 7,
    TeleportPylon = 8,
    Particles = 9,
    CreativePowerPermissions = 10,    
}
