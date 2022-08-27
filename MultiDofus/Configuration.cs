using MultiDofus.Framework;

namespace MultiDofus
{
    internal static class Configuration
    {
        public static Win32API.VirtualKeys NextWindowKey { get; set; } = Win32API.VirtualKeys.Tab;

        public static Win32API.VirtualKeys PreviousWindowKey { get; set; } = Win32API.VirtualKeys.Multiply;

        public static int DeathTimeSwap { get; set; } = 100;

        public static int DeathTimeClick { get; set; } = 80;
    }
}
