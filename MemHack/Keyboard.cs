using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MemHack
{
    class Keyboard
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImportAttribute("user32.dll")]
        public static extern uint MapVirtualKeyW(uint uCode, uint uMapType);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        public static List<Keys> keysDown = new List<Keys>();
        public static char lastKey;
        public static int lastKeyCode;

        public static bool IsKeyDown(Keys k)
        {
            return keysDown.Contains(k);
        }

        public static void HookKeyboard()
        {
            _hookID = SetHook(_proc);
            Application.Run();
        }

        public static void UnHookKeyBoard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (!keysDown.Contains((Keys)vkCode))
                {
                    keysDown.Add((Keys)vkCode);

                    lastKey = ((char)MapVirtualKeyW((uint)(Keys)vkCode, 0x02));
                    if (IsKeyDown(Keys.LShiftKey)) lastKey = Char.ToUpper(lastKey);
                    else lastKey = Char.ToLower(lastKey);
                    lastKeyCode = vkCode;
                }
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (keysDown.Contains((Keys)vkCode))
                {
                    keysDown.Remove((Keys)vkCode);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
