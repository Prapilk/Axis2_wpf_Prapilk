using System;

namespace Axis2.WPF.Services
{
    public static class SystemInfo
    {
        public static bool IsX64 => Environment.Is64BitProcess;
    }
}