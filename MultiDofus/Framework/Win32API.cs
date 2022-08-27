using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace MultiDofus.Framework
{
    internal static class Win32API
    {
        private static readonly VirtualKeys[] _virtualKeys;
        private static readonly ConcurrentDictionary<VirtualKeys, byte> _pressedKeys;
        private static readonly Timer _keyTimer;

        static Win32API()
        {
            _virtualKeys = Enum.GetValues<VirtualKeys>();
            _pressedKeys = new();
            _keyTimer = new(new(RegisterPressedKeys), default, 0, 1);
        }

        #region Events
        public static event Action<VirtualKeys>? OnKeyUp;
        public static event Action<VirtualKeys>? OnKeyDown;
        #endregion

        private static void RegisterPressedKeys(object? _)
        {
            foreach (var key in _virtualKeys)
                if (GetAsyncKeyState(key))
                {
                    OnKeyDown?.Invoke(key);

                    if (!_pressedKeys.ContainsKey(key))
                        _pressedKeys.TryAdd(key, default);
                }
                else if (_pressedKeys.TryRemove(key, out var __))
                    OnKeyUp?.Invoke(key);
        }

        [DllImport("User32.dll")]
        internal static extern IntPtr SetParent(IntPtr hwc, IntPtr hwp);

        /// <summary>
        /// This static method is required because legacy OSes do not support SetWindowLongPtr
        /// </summary>
        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern uint GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// This static method is required because Win32 does not support GetWindowLongPtr directly
        /// </summary>
        internal static uint GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool BringWindowToTop(HandleRef hWnd);

        [DllImport("user32")]
        internal static extern bool GetAsyncKeyState(VirtualKeys vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Point point);

        [DllImport("User32.dll")]
        internal static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        internal static extern void mouse_event(uint dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, IntPtr x, IntPtr y, IntPtr cx, IntPtr cy, IntPtr wFlags);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, SW nCmdShow);

        internal const int GWL_STYLE = -16;
        internal const int GWL_EXSTYLE = -20;
        internal const int MouseLeftButtonDown = 0x02;
        internal const int MouseLeftButtonUp = 0x04;

        [Flags]
        internal enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            //Extended Window Styles

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,

            //#if(WINVER >= 0x0400)

            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,

            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,

            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
            //#endif /* WINVER >= 0x0400 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_LAYERED = 0x00080000,
            //#endif /* WIN32WINNT >= 0x0500 */

            //#if(WINVER >= 0x0500)

            WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
            WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
                                          //#endif /* WINVER >= 0x0500 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000
            //#endif /* WIN32WINNT >= 0x0500 */
        }

        // An enumeration containing all the possible SW values.
        public enum SW : int
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10
        }

        internal enum VirtualKeys
        {
            LeftMouseButton = 0x01,
            RightMouseButton,
            Cancel,
            MiddleMouseButton,
            X1MouseButton,
            X2MouseButton,
            Back = 0x08,
            Tab,
            Clear = 0x0C,
            Enter,
            Shift = 0x10,
            Control,
            Alt,
            Pause,
            Capital,
            IMEKanaHanguelHangul,
            IMEOn,
            IMEJunja,
            IMEFinal,
            IMEHanjaKanji,
            IMEOff,
            Escape,
            IMEConvert,
            IMENonConvert,
            IMEAccept,
            IMEModeChange,
            Space,
            PageUp,
            PageDown,
            End,
            Home,
            LeftArrow,
            UpArrow,
            RightArrow,
            DownArrow,
            Select,
            Print,
            Execute,
            Snapshot,
            Insert,
            Delete,
            Help,
            Zero,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            A = 0x41,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            I,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
            Q,
            R,
            S,
            T,
            U,
            V,
            W,
            X,
            Y,
            Z,
            LeftWindows,
            RightWindows,
            Applications,
            ComputerSleep = 0x5F,
            NumericKeyPadZero,
            NumericKeyPadOne,
            NumericKeyPadTwo,
            NumericKeyPadThree,
            NumericKeyPadFour,
            NumericKeyPadFive,
            NumericKeyPadSix,
            NumericKeyPadSeven,
            NumericKeyPadEight,
            NumericKeyPadNine,
            Multiply,
            Add,
            Separator,
            Substract,
            Decimal,
            Divide,
            F1,
            F2,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            F10,
            F11,
            F12,
            F13,
            F14,
            F15,
            F16,
            F17,
            F18,
            F19,
            F20,
            F21,
            F22,
            F23,
            F24,
            NumLock = 0x90,
            Scroll,
            LeftShift = 0xA0,
            RightShift,
            LeftControl,
            RightControl,
            LeftAlt,
            RightAlt,
            BrowserBack,
            BrowserForward,
            BrowserRefresh,
            BrowserStop,
            BrowserSearch,
            BrowserFavorites,
            BrowserHome,
            VolumeMute,
            VolumeDown,
            VolumeUp,
            NextTrack,
            PreviousTrack,
            StopTrack,
            PlayPauseMedia,
            StartMail,
            SelectMedia,
            StartApp1,
            StartApp2,
            Miscellaneous = 0xBA,
            MiscellaneousPlus,
            MiscellaneousMinus,
            Miscellaneous2,
            Miscellaneous3,
            Miscellaneous4 = 0xDB,
            Miscellaneous5,
            Miscellaneous6,
            Miscellaneous7,
            Miscellaneous8,
            Miscellaneous102 = 0xE2,
            IMEProcess = 0xE5,
            Packet = 0xE7,
            Attn = 0xF6,
            CrSel,
            ExSel,
            EraseEOF,
            Play,
            Zoom,
            Pa1 = 0xFD,
            OEMClear,
        }
    }
}
