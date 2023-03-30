using System.Numerics;

namespace HeadlessTerrariaClient.Game;

public class Item
{
    public int Type;

    public int Stack;

    public int Prefix;

    public bool Active;

    public Vector2 Position;

    public Vector2 Velocity;

    public bool IsInstanced;

    public int PlayerItemInstanceIsReservedFor;

    public int NetID => Type switch
    {
        3521 => -1,
        3520 => -2,
        3519 => -3,
        3518 => -4,
        3517 => -5,
        3516 => -6,
        3515 => -7,
        3514 => -8,
        3513 => -9,
        3512 => -10,
        3511 => -11,
        3510 => -12,
        3509 => -13,
        3508 => -14,
        3507 => -15,
        3506 => -16,
        3505 => -17,
        3504 => -18,
        3764 => -19,
        3765 => -20,
        3766 => -21,
        3767 => -22,
        3768 => -23,
        3769 => -24,
        3503 => -25,
        3502 => -26,
        3501 => -27,
        3500 => -28,
        3499 => -29,
        3498 => -30,
        3497 => -31,
        3496 => -32,
        3495 => -33,
        3494 => -34,
        3493 => -35,
        3492 => -36,
        3491 => -37,
        3490 => -38,
        3489 => -39,
        3488 => -40,
        3487 => -41,
        3486 => -42,
        3485 => -43,
        3484 => -44,
        3483 => -45,
        3482 => -46,
        3481 => -47,
        3480 => -48,
        _ => Type,
    };

    public Item()
    {

    }

    public Item(int type, int stack = 1, int prefix = 0, bool active = false)
    {
        Type = type;
        Stack = stack;
        Prefix = prefix;
        Active = active;
    }

    public void SetType(int type)
    {
        Type = type;
    }

    public void SetTypeFromNetId(int netId)
    {
        switch (netId)
        {
            case -1:
                Type = 3521;
                return;
            case -2:
                Type = 3520;
                return;
            case -3:
                Type = 3519;
                return;
            case -4:
                Type = 3518;
                return;
            case -5:
                Type = 3517;
                return;
            case -6:
                Type = 3516;
                return;
            case -7:
                Type = 3515;
                return;
            case -8:
                Type = 3514;
                return;
            case -9:
                Type = 3513;
                return;
            case -10:
                Type = 3512;
                return;
            case -11:
                Type = 3511;
                return;
            case -12:
                Type = 3510;
                return;
            case -13:
                Type = 3509;
                return;
            case -14:
                Type = 3508;
                return;
            case -15:
                Type = 3507;
                return;
            case -16:
                Type = 3506;
                return;
            case -17:
                Type = 3505;
                return;
            case -18:
                Type = 3504;
                return;
            case -19:
                Type = 3764;
                return;
            case -20:
                Type = 3765;
                return;
            case -21:
                Type = 3766;
                return;
            case -22:
                Type = 3767;
                return;
            case -23:
                Type = 3768;
                return;
            case -24:
                Type = 3769;
                return;
            case -25:
                Type = 3503;
                return;
            case -26:
                Type = 3502;
                return;
            case -27:
                Type = 3501;
                return;
            case -28:
                Type = 3500;
                return;
            case -29:
                Type = 3499;
                return;
            case -30:
                Type = 3498;
                return;
            case -31:
                Type = 3497;
                return;
            case -32:
                Type = 3496;
                return;
            case -33:
                Type = 3495;
                return;
            case -34:
                Type = 3494;
                return;
            case -35:
                Type = 3493;
                return;
            case -36:
                Type = 3492;
                return;
            case -37:
                Type = 3491;
                return;
            case -38:
                Type = 3490;
                return;
            case -39:
                Type = 3489;
                return;
            case -40:
                Type = 3488;
                return;
            case -41:
                Type = 3487;
                return;
            case -42:
                Type = 3486;
                return;
            case -43:
                Type = 3485;
                return;
            case -44:
                Type = 3484;
                return;
            case -45:
                Type = 3483;
                return;
            case -46:
                Type = 3482;
                return;
            case -47:
                Type = 3481;
                return;
            case -48:
                Type = 3480;
                return;
        }

        if (netId >= 0)
        {
            Type = netId;
        }
    }

    /// <summary>
    /// Creates a copy of an item
    /// </summary>
    /// <returns>Copy of the item</returns>
    public Item Clone()
    {
        Item item = new Item(Type, Stack, Prefix, Active);

        item.Position = Position;

        item.Velocity = Velocity;

        return item;
    }
}
