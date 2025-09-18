using System;

namespace Axis2.WPF.Models
{
    [Flags]
    public enum AccountPrivileges : uint
    {
        None = 0x00,
        GM = 0x02,
        Page = 0x08,
        HearAll = 0x10,
        AllMove = 0x20,
        CombatDetail = 0x40,
        Debug = 0x80,
        ShowPriv = 0x200,
        Telnet = 0x400,
        Jailed = 0x800,
        Blocked = 0x2000,
        AllShow = 0x4000
    }
}
