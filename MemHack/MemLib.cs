using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MemLibs;
using SharpDX.Direct3D11;

namespace MemHack
{
    public class MemLib
    {
        public const uint PROCESS_VM_READ = (0x0010);
        public const uint PROCESS_VM_WRITE = (0x0020);
        public const uint PROCESS_VM_OPERATION = (0x0008);
        public const uint PAGE_READWRITE = 0x0004;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwAccess, bool inherit, int pid);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, [In, Out] byte[] lpBuffer, UInt64 dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, UInt64 dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, Int64 lpBaseAddress, [In, Out] byte[] lpBuffer, UInt64 dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, UInt32 dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UInt32 dwSize, uint flNewProtect, out uint lpflOldProtect);

        public static void SendInputWithAPI(ScanCodeShort keycode, KEYEVENTF ev)
        {
            MemLib.INPUT[] Inputs = new MemLib.INPUT[4];
            MemLib.INPUT Input = new MemLib.INPUT();

            Input.type = 1; // 1 = Keyboard Input
            Input.U.ki.wScan = keycode;
            Input.U.ki.dwFlags = ev;
            Inputs[0] = Input;

            SendInput(1, Inputs, MemLib.INPUT.Size);
        }

        /// <summary>
        /// Declaration of external SendInput method
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern uint SendInput(
            uint nInputs,
            [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
            int cbSize);


        // Declare the INPUT struct
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            internal uint type;
            internal InputUnion U;
            internal static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }

        // Declare the InputUnion struct
        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal MouseEventDataXButtons mouseData;
            internal MOUSEEVENTF dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [Flags]
        internal enum MouseEventDataXButtons : uint
        {
            Nothing = 0x00000000,
            XBUTTON1 = 0x00000001,
            XBUTTON2 = 0x00000002
        }

        [Flags]
        internal enum MOUSEEVENTF : uint
        {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal VirtualKeyShort wVk;
            internal ScanCodeShort wScan;
            internal KEYEVENTF dwFlags;
            internal int time;
            internal UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }

        internal enum VirtualKeyShort : short
        {
            ///<summary>
            ///Left mouse button
            ///</summary>
            LBUTTON = 0x01,
            ///<summary>
            ///Right mouse button
            ///</summary>
            RBUTTON = 0x02,
            ///<summary>
            ///Control-break processing
            ///</summary>
            CANCEL = 0x03,
            ///<summary>
            ///Middle mouse button (three-button mouse)
            ///</summary>
            MBUTTON = 0x04,
            ///<summary>
            ///Windows 2000/XP: X1 mouse button
            ///</summary>
            XBUTTON1 = 0x05,
            ///<summary>
            ///Windows 2000/XP: X2 mouse button
            ///</summary>
            XBUTTON2 = 0x06,
            ///<summary>
            ///BACKSPACE key
            ///</summary>
            BACK = 0x08,
            ///<summary>
            ///TAB key
            ///</summary>
            TAB = 0x09,
            ///<summary>
            ///CLEAR key
            ///</summary>
            CLEAR = 0x0C,
            ///<summary>
            ///ENTER key
            ///</summary>
            RETURN = 0x0D,
            ///<summary>
            ///SHIFT key
            ///</summary>
            SHIFT = 0x10,
            ///<summary>
            ///CTRL key
            ///</summary>
            CONTROL = 0x11,
            ///<summary>
            ///ALT key
            ///</summary>
            MENU = 0x12,
            ///<summary>
            ///PAUSE key
            ///</summary>
            PAUSE = 0x13,
            ///<summary>
            ///CAPS LOCK key
            ///</summary>
            CAPITAL = 0x14,
            ///<summary>
            ///Input Method Editor (IME) Kana mode
            ///</summary>
            KANA = 0x15,
            ///<summary>
            ///IME Hangul mode
            ///</summary>
            HANGUL = 0x15,
            ///<summary>
            ///IME Junja mode
            ///</summary>
            JUNJA = 0x17,
            ///<summary>
            ///IME final mode
            ///</summary>
            FINAL = 0x18,
            ///<summary>
            ///IME Hanja mode
            ///</summary>
            HANJA = 0x19,
            ///<summary>
            ///IME Kanji mode
            ///</summary>
            KANJI = 0x19,
            ///<summary>
            ///ESC key
            ///</summary>
            ESCAPE = 0x1B,
            ///<summary>
            ///IME convert
            ///</summary>
            CONVERT = 0x1C,
            ///<summary>
            ///IME nonconvert
            ///</summary>
            NONCONVERT = 0x1D,
            ///<summary>
            ///IME accept
            ///</summary>
            ACCEPT = 0x1E,
            ///<summary>
            ///IME mode change request
            ///</summary>
            MODECHANGE = 0x1F,
            ///<summary>
            ///SPACEBAR
            ///</summary>
            SPACE = 0x20,
            ///<summary>
            ///PAGE UP key
            ///</summary>
            PRIOR = 0x21,
            ///<summary>
            ///PAGE DOWN key
            ///</summary>
            NEXT = 0x22,
            ///<summary>
            ///END key
            ///</summary>
            END = 0x23,
            ///<summary>
            ///HOME key
            ///</summary>
            HOME = 0x24,
            ///<summary>
            ///LEFT ARROW key
            ///</summary>
            LEFT = 0x25,
            ///<summary>
            ///UP ARROW key
            ///</summary>
            UP = 0x26,
            ///<summary>
            ///RIGHT ARROW key
            ///</summary>
            RIGHT = 0x27,
            ///<summary>
            ///DOWN ARROW key
            ///</summary>
            DOWN = 0x28,
            ///<summary>
            ///SELECT key
            ///</summary>
            SELECT = 0x29,
            ///<summary>
            ///PRINT key
            ///</summary>
            PRINT = 0x2A,
            ///<summary>
            ///EXECUTE key
            ///</summary>
            EXECUTE = 0x2B,
            ///<summary>
            ///PRINT SCREEN key
            ///</summary>
            SNAPSHOT = 0x2C,
            ///<summary>
            ///INS key
            ///</summary>
            INSERT = 0x2D,
            ///<summary>
            ///DEL key
            ///</summary>
            DELETE = 0x2E,
            ///<summary>
            ///HELP key
            ///</summary>
            HELP = 0x2F,
            ///<summary>
            ///0 key
            ///</summary>
            KEY_0 = 0x30,
            ///<summary>
            ///1 key
            ///</summary>
            KEY_1 = 0x31,
            ///<summary>
            ///2 key
            ///</summary>
            KEY_2 = 0x32,
            ///<summary>
            ///3 key
            ///</summary>
            KEY_3 = 0x33,
            ///<summary>
            ///4 key
            ///</summary>
            KEY_4 = 0x34,
            ///<summary>
            ///5 key
            ///</summary>
            KEY_5 = 0x35,
            ///<summary>
            ///6 key
            ///</summary>
            KEY_6 = 0x36,
            ///<summary>
            ///7 key
            ///</summary>
            KEY_7 = 0x37,
            ///<summary>
            ///8 key
            ///</summary>
            KEY_8 = 0x38,
            ///<summary>
            ///9 key
            ///</summary>
            KEY_9 = 0x39,
            ///<summary>
            ///A key
            ///</summary>
            KEY_A = 0x41,
            ///<summary>
            ///B key
            ///</summary>
            KEY_B = 0x42,
            ///<summary>
            ///C key
            ///</summary>
            KEY_C = 0x43,
            ///<summary>
            ///D key
            ///</summary>
            KEY_D = 0x44,
            ///<summary>
            ///E key
            ///</summary>
            KEY_E = 0x45,
            ///<summary>
            ///F key
            ///</summary>
            KEY_F = 0x46,
            ///<summary>
            ///G key
            ///</summary>
            KEY_G = 0x47,
            ///<summary>
            ///H key
            ///</summary>
            KEY_H = 0x48,
            ///<summary>
            ///I key
            ///</summary>
            KEY_I = 0x49,
            ///<summary>
            ///J key
            ///</summary>
            KEY_J = 0x4A,
            ///<summary>
            ///K key
            ///</summary>
            KEY_K = 0x4B,
            ///<summary>
            ///L key
            ///</summary>
            KEY_L = 0x4C,
            ///<summary>
            ///M key
            ///</summary>
            KEY_M = 0x4D,
            ///<summary>
            ///N key
            ///</summary>
            KEY_N = 0x4E,
            ///<summary>
            ///O key
            ///</summary>
            KEY_O = 0x4F,
            ///<summary>
            ///P key
            ///</summary>
            KEY_P = 0x50,
            ///<summary>
            ///Q key
            ///</summary>
            KEY_Q = 0x51,
            ///<summary>
            ///R key
            ///</summary>
            KEY_R = 0x52,
            ///<summary>
            ///S key
            ///</summary>
            KEY_S = 0x53,
            ///<summary>
            ///T key
            ///</summary>
            KEY_T = 0x54,
            ///<summary>
            ///U key
            ///</summary>
            KEY_U = 0x55,
            ///<summary>
            ///V key
            ///</summary>
            KEY_V = 0x56,
            ///<summary>
            ///W key
            ///</summary>
            KEY_W = 0x57,
            ///<summary>
            ///X key
            ///</summary>
            KEY_X = 0x58,
            ///<summary>
            ///Y key
            ///</summary>
            KEY_Y = 0x59,
            ///<summary>
            ///Z key
            ///</summary>
            KEY_Z = 0x5A,
            ///<summary>
            ///Left Windows key (Microsoft Natural keyboard) 
            ///</summary>
            LWIN = 0x5B,
            ///<summary>
            ///Right Windows key (Natural keyboard)
            ///</summary>
            RWIN = 0x5C,
            ///<summary>
            ///Applications key (Natural keyboard)
            ///</summary>
            APPS = 0x5D,
            ///<summary>
            ///Computer Sleep key
            ///</summary>
            SLEEP = 0x5F,
            ///<summary>
            ///Numeric keypad 0 key
            ///</summary>
            NUMPAD0 = 0x60,
            ///<summary>
            ///Numeric keypad 1 key
            ///</summary>
            NUMPAD1 = 0x61,
            ///<summary>
            ///Numeric keypad 2 key
            ///</summary>
            NUMPAD2 = 0x62,
            ///<summary>
            ///Numeric keypad 3 key
            ///</summary>
            NUMPAD3 = 0x63,
            ///<summary>
            ///Numeric keypad 4 key
            ///</summary>
            NUMPAD4 = 0x64,
            ///<summary>
            ///Numeric keypad 5 key
            ///</summary>
            NUMPAD5 = 0x65,
            ///<summary>
            ///Numeric keypad 6 key
            ///</summary>
            NUMPAD6 = 0x66,
            ///<summary>
            ///Numeric keypad 7 key
            ///</summary>
            NUMPAD7 = 0x67,
            ///<summary>
            ///Numeric keypad 8 key
            ///</summary>
            NUMPAD8 = 0x68,
            ///<summary>
            ///Numeric keypad 9 key
            ///</summary>
            NUMPAD9 = 0x69,
            ///<summary>
            ///Multiply key
            ///</summary>
            MULTIPLY = 0x6A,
            ///<summary>
            ///Add key
            ///</summary>
            ADD = 0x6B,
            ///<summary>
            ///Separator key
            ///</summary>
            SEPARATOR = 0x6C,
            ///<summary>
            ///Subtract key
            ///</summary>
            SUBTRACT = 0x6D,
            ///<summary>
            ///Decimal key
            ///</summary>
            DECIMAL = 0x6E,
            ///<summary>
            ///Divide key
            ///</summary>
            DIVIDE = 0x6F,
            ///<summary>
            ///F1 key
            ///</summary>
            F1 = 0x70,
            ///<summary>
            ///F2 key
            ///</summary>
            F2 = 0x71,
            ///<summary>
            ///F3 key
            ///</summary>
            F3 = 0x72,
            ///<summary>
            ///F4 key
            ///</summary>
            F4 = 0x73,
            ///<summary>
            ///F5 key
            ///</summary>
            F5 = 0x74,
            ///<summary>
            ///F6 key
            ///</summary>
            F6 = 0x75,
            ///<summary>
            ///F7 key
            ///</summary>
            F7 = 0x76,
            ///<summary>
            ///F8 key
            ///</summary>
            F8 = 0x77,
            ///<summary>
            ///F9 key
            ///</summary>
            F9 = 0x78,
            ///<summary>
            ///F10 key
            ///</summary>
            F10 = 0x79,
            ///<summary>
            ///F11 key
            ///</summary>
            F11 = 0x7A,
            ///<summary>
            ///F12 key
            ///</summary>
            F12 = 0x7B,
            ///<summary>
            ///F13 key
            ///</summary>
            F13 = 0x7C,
            ///<summary>
            ///F14 key
            ///</summary>
            F14 = 0x7D,
            ///<summary>
            ///F15 key
            ///</summary>
            F15 = 0x7E,
            ///<summary>
            ///F16 key
            ///</summary>
            F16 = 0x7F,
            ///<summary>
            ///F17 key  
            ///</summary>
            F17 = 0x80,
            ///<summary>
            ///F18 key  
            ///</summary>
            F18 = 0x81,
            ///<summary>
            ///F19 key  
            ///</summary>
            F19 = 0x82,
            ///<summary>
            ///F20 key  
            ///</summary>
            F20 = 0x83,
            ///<summary>
            ///F21 key  
            ///</summary>
            F21 = 0x84,
            ///<summary>
            ///F22 key, (PPC only) Key used to lock device.
            ///</summary>
            F22 = 0x85,
            ///<summary>
            ///F23 key  
            ///</summary>
            F23 = 0x86,
            ///<summary>
            ///F24 key  
            ///</summary>
            F24 = 0x87,
            ///<summary>
            ///NUM LOCK key
            ///</summary>
            NUMLOCK = 0x90,
            ///<summary>
            ///SCROLL LOCK key
            ///</summary>
            SCROLL = 0x91,
            ///<summary>
            ///Left SHIFT key
            ///</summary>
            LSHIFT = 0xA0,
            ///<summary>
            ///Right SHIFT key
            ///</summary>
            RSHIFT = 0xA1,
            ///<summary>
            ///Left CONTROL key
            ///</summary>
            LCONTROL = 0xA2,
            ///<summary>
            ///Right CONTROL key
            ///</summary>
            RCONTROL = 0xA3,
            ///<summary>
            ///Left MENU key
            ///</summary>
            LMENU = 0xA4,
            ///<summary>
            ///Right MENU key
            ///</summary>
            RMENU = 0xA5,
            ///<summary>
            ///Windows 2000/XP: Browser Back key
            ///</summary>
            BROWSER_BACK = 0xA6,
            ///<summary>
            ///Windows 2000/XP: Browser Forward key
            ///</summary>
            BROWSER_FORWARD = 0xA7,
            ///<summary>
            ///Windows 2000/XP: Browser Refresh key
            ///</summary>
            BROWSER_REFRESH = 0xA8,
            ///<summary>
            ///Windows 2000/XP: Browser Stop key
            ///</summary>
            BROWSER_STOP = 0xA9,
            ///<summary>
            ///Windows 2000/XP: Browser Search key 
            ///</summary>
            BROWSER_SEARCH = 0xAA,
            ///<summary>
            ///Windows 2000/XP: Browser Favorites key
            ///</summary>
            BROWSER_FAVORITES = 0xAB,
            ///<summary>
            ///Windows 2000/XP: Browser Start and Home key
            ///</summary>
            BROWSER_HOME = 0xAC,
            ///<summary>
            ///Windows 2000/XP: Volume Mute key
            ///</summary>
            VOLUME_MUTE = 0xAD,
            ///<summary>
            ///Windows 2000/XP: Volume Down key
            ///</summary>
            VOLUME_DOWN = 0xAE,
            ///<summary>
            ///Windows 2000/XP: Volume Up key
            ///</summary>
            VOLUME_UP = 0xAF,
            ///<summary>
            ///Windows 2000/XP: Next Track key
            ///</summary>
            MEDIA_NEXT_TRACK = 0xB0,
            ///<summary>
            ///Windows 2000/XP: Previous Track key
            ///</summary>
            MEDIA_PREV_TRACK = 0xB1,
            ///<summary>
            ///Windows 2000/XP: Stop Media key
            ///</summary>
            MEDIA_STOP = 0xB2,
            ///<summary>
            ///Windows 2000/XP: Play/Pause Media key
            ///</summary>
            MEDIA_PLAY_PAUSE = 0xB3,
            ///<summary>
            ///Windows 2000/XP: Start Mail key
            ///</summary>
            LAUNCH_MAIL = 0xB4,
            ///<summary>
            ///Windows 2000/XP: Select Media key
            ///</summary>
            LAUNCH_MEDIA_SELECT = 0xB5,
            ///<summary>
            ///Windows 2000/XP: Start Application 1 key
            ///</summary>
            LAUNCH_APP1 = 0xB6,
            ///<summary>
            ///Windows 2000/XP: Start Application 2 key
            ///</summary>
            LAUNCH_APP2 = 0xB7,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_1 = 0xBA,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '+' key
            ///</summary>
            OEM_PLUS = 0xBB,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the ',' key
            ///</summary>
            OEM_COMMA = 0xBC,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '-' key
            ///</summary>
            OEM_MINUS = 0xBD,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '.' key
            ///</summary>
            OEM_PERIOD = 0xBE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_2 = 0xBF,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_3 = 0xC0,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_4 = 0xDB,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_5 = 0xDC,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_6 = 0xDD,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_7 = 0xDE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_8 = 0xDF,
            ///<summary>
            ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
            ///</summary>
            OEM_102 = 0xE2,
            ///<summary>
            ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            ///</summary>
            PROCESSKEY = 0xE5,
            ///<summary>
            ///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
            ///The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
            ///see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            ///</summary>
            PACKET = 0xE7,
            ///<summary>
            ///Attn key
            ///</summary>
            ATTN = 0xF6,
            ///<summary>
            ///CrSel key
            ///</summary>
            CRSEL = 0xF7,
            ///<summary>
            ///ExSel key
            ///</summary>
            EXSEL = 0xF8,
            ///<summary>
            ///Erase EOF key
            ///</summary>
            EREOF = 0xF9,
            ///<summary>
            ///Play key
            ///</summary>
            PLAY = 0xFA,
            ///<summary>
            ///Zoom key
            ///</summary>
            ZOOM = 0xFB,
            ///<summary>
            ///Reserved 
            ///</summary>
            NONAME = 0xFC,
            ///<summary>
            ///PA1 key
            ///</summary>
            PA1 = 0xFD,
            ///<summary>
            ///Clear key
            ///</summary>
            OEM_CLEAR = 0xFE
        }

        public enum ScanCodeShort : short
        {
            LBUTTON = 0,
            RBUTTON = 0,
            CANCEL = 70,
            MBUTTON = 0,
            XBUTTON1 = 0,
            XBUTTON2 = 0,
            BACK = 14,
            TAB = 15,
            CLEAR = 76,
            RETURN = 28,
            SHIFT = 42,
            CONTROL = 29,
            MENU = 56,
            PAUSE = 0,
            CAPITAL = 58,
            KANA = 0,
            HANGUL = 0,
            JUNJA = 0,
            FINAL = 0,
            HANJA = 0,
            KANJI = 0,
            ESCAPE = 1,
            CONVERT = 0,
            NONCONVERT = 0,
            ACCEPT = 0,
            MODECHANGE = 0,
            SPACE = 57,
            PRIOR = 73,
            NEXT = 81,
            END = 79,
            HOME = 71,
            LEFT = 75,
            UP = 72,
            RIGHT = 77,
            DOWN = 80,
            SELECT = 0,
            PRINT = 0,
            EXECUTE = 0,
            SNAPSHOT = 84,
            INSERT = 82,
            DELETE = 83,
            HELP = 99,
            KEY_0 = 11,
            KEY_1 = 2,
            KEY_2 = 3,
            KEY_3 = 4,
            KEY_4 = 5,
            KEY_5 = 6,
            KEY_6 = 7,
            KEY_7 = 8,
            KEY_8 = 9,
            KEY_9 = 10,
            KEY_A = 30,
            KEY_B = 48,
            KEY_C = 46,
            KEY_D = 32,
            KEY_E = 18,
            KEY_F = 33,
            KEY_G = 34,
            KEY_H = 35,
            KEY_I = 23,
            KEY_J = 36,
            KEY_K = 37,
            KEY_L = 38,
            KEY_M = 50,
            KEY_N = 49,
            KEY_O = 24,
            KEY_P = 25,
            KEY_Q = 16,
            KEY_R = 19,
            KEY_S = 31,
            KEY_T = 20,
            KEY_U = 22,
            KEY_V = 47,
            KEY_W = 17,
            KEY_X = 45,
            KEY_Y = 21,
            KEY_Z = 44,
            LWIN = 91,
            RWIN = 92,
            APPS = 93,
            SLEEP = 95,
            NUMPAD0 = 82,
            NUMPAD1 = 79,
            NUMPAD2 = 80,
            NUMPAD3 = 81,
            NUMPAD4 = 75,
            NUMPAD5 = 76,
            NUMPAD6 = 77,
            NUMPAD7 = 71,
            NUMPAD8 = 72,
            NUMPAD9 = 73,
            MULTIPLY = 55,
            ADD = 78,
            SEPARATOR = 0,
            SUBTRACT = 74,
            DECIMAL = 83,
            DIVIDE = 53,
            F1 = 59,
            F2 = 60,
            F3 = 61,
            F4 = 62,
            F5 = 63,
            F6 = 64,
            F7 = 65,
            F8 = 66,
            F9 = 67,
            F10 = 68,
            F11 = 87,
            F12 = 88,
            F13 = 100,
            F14 = 101,
            F15 = 102,
            F16 = 103,
            F17 = 104,
            F18 = 105,
            F19 = 106,
            F20 = 107,
            F21 = 108,
            F22 = 109,
            F23 = 110,
            F24 = 118,
            NUMLOCK = 69,
            SCROLL = 70,
            LSHIFT = 42,
            RSHIFT = 54,
            LCONTROL = 29,
            RCONTROL = 29,
            LMENU = 56,
            RMENU = 56,
            BROWSER_BACK = 106,
            BROWSER_FORWARD = 105,
            BROWSER_REFRESH = 103,
            BROWSER_STOP = 104,
            BROWSER_SEARCH = 101,
            BROWSER_FAVORITES = 102,
            BROWSER_HOME = 50,
            VOLUME_MUTE = 32,
            VOLUME_DOWN = 46,
            VOLUME_UP = 48,
            MEDIA_NEXT_TRACK = 25,
            MEDIA_PREV_TRACK = 16,
            MEDIA_STOP = 36,
            MEDIA_PLAY_PAUSE = 34,
            LAUNCH_MAIL = 108,
            LAUNCH_MEDIA_SELECT = 109,
            LAUNCH_APP1 = 107,
            LAUNCH_APP2 = 33,
            OEM_1 = 39,
            OEM_PLUS = 13,
            OEM_COMMA = 51,
            OEM_MINUS = 12,
            OEM_PERIOD = 52,
            OEM_2 = 53,
            OEM_3 = 41,
            OEM_4 = 26,
            OEM_5 = 43,
            OEM_6 = 27,
            OEM_7 = 40,
            OEM_8 = 0,
            OEM_102 = 86,
            PROCESSKEY = 0,
            PACKET = 0,
            ATTN = 0,
            CRSEL = 0,
            EXSEL = 0,
            EREOF = 93,
            PLAY = 0,
            ZOOM = 98,
            NONAME = 0,
            PA1 = 0,
            OEM_CLEAR = 0,
        }

        /// <summary>
        /// Define HARDWAREINPUT struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            internal int uMsg;
            internal short wParamL;
            internal short wParamH;
        }
    }

    public class Memory
    {
        private Process CurProcess;
        private IntPtr ProcessHandle;
        private string ProcessName;
        public int ProcessID;
        public IntPtr BaseModule;

        // Destruktor
        ~Memory() { if (ProcessHandle != IntPtr.Zero) MemLib.CloseHandle(ProcessHandle); }

        // Get Process for work
        public bool AttackProcess(string _ProcessName)
        {
            try
            {
                Process[] Processes = Process.GetProcessesByName(_ProcessName);

                if (Processes.Length > 0)
                {
                    BaseModule = Processes[0].MainModule.BaseAddress;
                    CurProcess = Processes[0];
                    ProcessID = Processes[0].Id;
                    ProcessName = _ProcessName;
                    ProcessHandle = MemLib.OpenProcess(MemLib.PROCESS_VM_READ | MemLib.PROCESS_VM_WRITE | MemLib.PROCESS_VM_OPERATION, false, ProcessID);

                    if (ProcessHandle != IntPtr.Zero)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // If Process attacked
        public bool IsOpen()
        {
            if (ProcessName == string.Empty)
            {
                return false;
            }
            else
            {
                if (AttackProcess(ProcessName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        #region |- READ MEMORY -|

        // Read Object
        public object ReadObject(Int64 _lpBaseAddress, uint _Size)
        {
            object[] Buffer = new object[_Size];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, _Size, out ByteRead);

            return Buffer;
        }

        // Read Byte
        public byte ReadByte(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[sizeof(byte)];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, sizeof(byte), out ByteRead);

            return Buffer[0];
        }

        // Read Int32
        public Int32 ReadInt32(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[4];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, 4, out ByteRead);

            return BitConverter.ToInt32(Buffer, 0);
        }

        // Read UInt32
        public UInt32 ReadUInt32(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[4];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, 4, out ByteRead);

            return BitConverter.ToUInt32(Buffer, 0);
        }

        // Read Int64
        public Int64 ReadInt64(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[8];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, 8, out ByteRead);

            return BitConverter.ToInt64(Buffer, 0);
        }

        // Read UInt64
        public UInt64 ReadUInt64(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[8];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, 8, out ByteRead);

            return BitConverter.ToUInt64(Buffer, 0);
        }

        // Read Int64
        public float ReadFloat(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[sizeof(float)];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, sizeof(float), out ByteRead);

            return BitConverter.ToSingle(Buffer, 0);
        }

        // Read String
        public string ReadString(Int64 _lpBaseAddress, UInt64 _Size)
        {
            byte[] Buffer = new byte[_Size];
            IntPtr BytesRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, _Size, out BytesRead);

            return Encoding.UTF8.GetString(Buffer);
        }

        // Read Matrix4x4
        public Matrix4x4 ReadMatrix4x4(Int64 _lpBaseAddress)
        {
            Matrix4x4 tmp = new Matrix4x4();

            byte[] Buffer = new byte[64];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _lpBaseAddress, Buffer, 64, out ByteRead);

            tmp.M11 = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.M12 = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.M13 = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.M14 = BitConverter.ToSingle(Buffer, (3 * 4));

            tmp.M21 = BitConverter.ToSingle(Buffer, (4 * 4));
            tmp.M22 = BitConverter.ToSingle(Buffer, (5 * 4));
            tmp.M23 = BitConverter.ToSingle(Buffer, (6 * 4));
            tmp.M24 = BitConverter.ToSingle(Buffer, (7 * 4));

            tmp.M31 = BitConverter.ToSingle(Buffer, (8 * 4));
            tmp.M32 = BitConverter.ToSingle(Buffer, (9 * 4));
            tmp.M33 = BitConverter.ToSingle(Buffer, (10 * 4));
            tmp.M34 = BitConverter.ToSingle(Buffer, (11 * 4));

            tmp.M41 = BitConverter.ToSingle(Buffer, (12 * 4));
            tmp.M42 = BitConverter.ToSingle(Buffer, (13 * 4));
            tmp.M43 = BitConverter.ToSingle(Buffer, (14 * 4));
            tmp.M44 = BitConverter.ToSingle(Buffer, (15 * 4));
            return tmp;
        }

        // Read QuatTransform
        public QuatTransform ReadQuatTransform(Int64 _QuatTransformAddress)
        {
            QuatTransform QT = new QuatTransform();

            byte[] Buffer = new byte[20];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _QuatTransformAddress, Buffer, 20, out ByteRead);

            Vector4D TransAndScale = new Vector4D();
            Vector4D Rotation = new Vector4D();

            TransAndScale.X = BitConverter.ToSingle(Buffer, (0 * 4));
            TransAndScale.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            TransAndScale.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            TransAndScale.O = BitConverter.ToSingle(Buffer, (3 * 4));

            Rotation.X = BitConverter.ToSingle(Buffer, (4 * 4));
            Rotation.Y = BitConverter.ToSingle(Buffer, (5 * 4));
            Rotation.Z = BitConverter.ToSingle(Buffer, (6 * 4));
            Rotation.O = BitConverter.ToSingle(Buffer, (7 * 4));

            QT.TransAndScale = TransAndScale;
            QT.Rotation = Rotation;

            return QT;
        }

        public void WriteAngle(float _Yaw, float _Pitch)
        {
            Int64 pBase = ReadInt64(sAddresses.ViewAngles);
            Int64 pOffset1 = ReadInt64(pBase + 0x4988);
            Int64 pOffset2 = ReadInt64(pOffset1 + 0x10);

            WriteFloat(pOffset2 + 0x14, _Yaw);
            WriteFloat(pOffset2 + 0x18, _Pitch);
        }

        // Read BoneTransformInfo
        public BoneTransformInfo ReadBoneTransformInfo(Int64 _BoneTransformAddress)
        {
            BoneTransformInfo Info = new BoneTransformInfo();

            Info.Transform = ReadMatrix4x4(_BoneTransformAddress);

            byte[] Buffer = new byte[10];
            IntPtr ByteRead;

            MemLib.ReadProcessMemory(ProcessHandle, _BoneTransformAddress + 0x40, Buffer, 10, out ByteRead);

            // MATRIX4x4 -> START
            //BitConverter.ToSingle(Buffer, (0 * 4)); // BASE
            //BitConverter.ToSingle(Buffer, (1 * 4)); // +0x4
            //BitConverter.ToSingle(Buffer, (2 * 4)); // +0x8
            //BitConverter.ToSingle(Buffer, (3 * 4)); // +0xC
            //BitConverter.ToSingle(Buffer, (4 * 4)); // +0x10
            //BitConverter.ToSingle(Buffer, (5 * 4)); // +0x14
            //BitConverter.ToSingle(Buffer, (6 * 4)); // +0x18
            //BitConverter.ToSingle(Buffer, (7 * 4)); // +0x1C
            //BitConverter.ToSingle(Buffer, (8 * 4)); // +0x20
            //BitConverter.ToSingle(Buffer, (9 * 4)); // +0x24
            //BitConverter.ToSingle(Buffer, (10 * 4)); // +0x28
            //BitConverter.ToSingle(Buffer, (11 * 4)); // +0x2C
            //BitConverter.ToSingle(Buffer, (12 * 4)); // +0x30
            //BitConverter.ToSingle(Buffer, (13 * 4)); // +0x34
            //BitConverter.ToSingle(Buffer, (14 * 4)); // +0x38
            //BitConverter.ToSingle(Buffer, (15 * 4)); // +0x3C
            // MATRIX4x4 -> END

            //Info.Position.X = BitConverter.ToSingle(Buffer, (16 * 4)); // +0x40
            //Info.Position.Y = BitConverter.ToSingle(Buffer, (17 * 4)); // +0x44
            //Info.Position.Z = BitConverter.ToSingle(Buffer, (18 * 4)); // +0x48
            //Info.Position.O = BitConverter.ToSingle(Buffer, (19 * 4)); // +0x4C

            return Info;
        }

        #endregion


        #region |- WRITE MEMORY -|

        public void WriteMemory(Int64 MemoryAddress, byte[] Buffer)
        {
            uint oldProtect;
            MemLib.VirtualProtectEx(ProcessHandle, (IntPtr)MemoryAddress, (uint)Buffer.Length, MemLib.PAGE_READWRITE, out oldProtect);

            IntPtr ptrBytesWritten;
            MemLib.WriteProcessMemory(ProcessHandle, MemoryAddress, Buffer, (uint)Buffer.Length, out ptrBytesWritten);
        }

        public void WriteByte(Int64 _lpBaseAddress, byte[] Buffer)
        {
            WriteMemory(_lpBaseAddress, Buffer);
        }

        public void WriteInt32(Int64 _lpBaseAddress, int _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);

            WriteMemory(_lpBaseAddress, Buffer);
        }

        public void WriteUInt32(Int64 _lpBaseAddress, uint _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);

            WriteMemory(_lpBaseAddress, Buffer);
        }

        public void WriteFloat(Int64 _lpBaseAddress, float _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);

            WriteMemory(_lpBaseAddress, Buffer);
        }

        #endregion
    }
}