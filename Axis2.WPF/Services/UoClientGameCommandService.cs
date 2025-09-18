using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Axis2.WPF.Services
{
    public class UoClientGameCommandService : IGameCommandService
    {
        private const int kDelayKeystrokes = 45;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private IntPtr hwndUOClient;

        public void SendCommand(string command)
        {
            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                string title = sb.ToString();

                if (title.Contains("Ultima Online") || title.Contains("UOSA -") || title.Contains("ClassicUO"))
                {
                    hwndUOClient = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (hwndUOClient != IntPtr.Zero)
            {
                SetForegroundWindow(hwndUOClient);

                foreach (char c in command)
                {
                    SendMessage(hwndUOClient, 0x0100, (IntPtr)c, IntPtr.Zero); // WM_KEYDOWN
                    Thread.Sleep(kDelayKeystrokes / 2);
                    SendMessage(hwndUOClient, 0x0101, (IntPtr)c, IntPtr.Zero); // WM_KEYUP
                }

                SendMessage(hwndUOClient, 0x0100, (IntPtr)0x0D, IntPtr.Zero); // VK_RETURN
                Thread.Sleep(kDelayKeystrokes / 2);
                SendMessage(hwndUOClient, 0x0101, (IntPtr)0x0D, IntPtr.Zero);
            }
        }
    }
}
