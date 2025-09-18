using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Axis2.WPF.Services
{
    public class UoClientCommunicator
    {
        private const int kDelayKeystrokes = 0;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int VK_RETURN = 0x0D;

        // Messages Windows sp�cifiques
        private const uint WM_CHAR = 0x0102;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        private IntPtr hwndUOClient;
        private readonly string _commandPrefix;
        private readonly string _uoTitle;

        public UoClientCommunicator(string commandPrefix, string uoTitle)
        {
            _commandPrefix = commandPrefix;
            _uoTitle = uoTitle;
        }

        private bool ForceSetForegroundWindow(IntPtr hWnd)
        {
            try
            {
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
                else
                {
                    ShowWindow(hWnd, SW_SHOW);
                }

                uint foregroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
                uint targetThreadId = GetWindowThreadProcessId(hWnd, out _);
                uint currentThreadId = GetCurrentThreadId();

                if (foregroundThreadId != currentThreadId)
                {
                    AttachThreadInput(currentThreadId, foregroundThreadId, true);
                }

                if (targetThreadId != currentThreadId)
                {
                    AttachThreadInput(currentThreadId, targetThreadId, true);
                }

                BringWindowToTop(hWnd);
                bool result = SetForegroundWindow(hWnd);

                if (foregroundThreadId != currentThreadId)
                {
                    AttachThreadInput(currentThreadId, foregroundThreadId, false);
                }

                if (targetThreadId != currentThreadId)
                {
                    AttachThreadInput(currentThreadId, targetThreadId, false);
                }

                return result;
            }
            catch (Exception)
            {
                //System.Windows.MessageBox.Show($"Erreur dans ForceSetForegroundWindow: {ex.Message}", "Debug - Erreur");
                return false;
            }
        }

        private async Task<bool> SendToOrionDirectMessage(string command)
        {
            try
            {
                string finalCommand = string.IsNullOrEmpty(_commandPrefix) ? command : _commandPrefix + command;
                //System.Windows.MessageBox.Show($"DirectMessage - Commande: '{finalCommand}' vers handle {hwndUOClient}", "Debug - DirectMessage");

                // Cette m�thode envoie directement � la fen�tre, m�me si elle n'a pas le focus
                // Ouvrir le chat avec Enter
                SendMessage(hwndUOClient, WM_KEYDOWN, (IntPtr)VK_RETURN, (IntPtr)(1 | (28 << 16)));
                await Task.Delay(10);
                SendMessage(hwndUOClient, WM_KEYUP, (IntPtr)VK_RETURN, (IntPtr)(1 | (28 << 16) | (1 << 30) | (1 << 31)));

                await Task.Delay(50);

                // Envoyer chaque caract�re directement � la fen�tre
                foreach (char c in finalCommand)
                {
                    //System.Windows.MessageBox.Show($"Envoi direct du caract�re: '{c}' � Orion", $"Debug - Char {c}");
                    SendMessage(hwndUOClient, WM_CHAR, (IntPtr)c, (IntPtr)1);
                    await Task.Delay(kDelayKeystrokes);
                }

                // Enter final
                SendMessage(hwndUOClient, WM_KEYDOWN, (IntPtr)VK_RETURN, (IntPtr)(1 | (28 << 16)));
                await Task.Delay(10);
                SendMessage(hwndUOClient, WM_KEYUP, (IntPtr)VK_RETURN, (IntPtr)(1 | (28 << 16) | (1 << 30) | (1 << 31)));

                //System.Windows.MessageBox.Show("DirectMessage termin�!", "Debug - DirectMessage Fin");
                return true;
            }
            catch (Exception)
            {
                //System.Windows.MessageBox.Show($"Erreur DirectMessage: {ex.Message}", "Debug - Erreur DirectMessage");
                return false;
            }
        }

        public async Task<bool> SendToUOAsync(string command)
        {
            //System.Windows.MessageBox.Show($"D�but SendToUOAsync ORION avec commande: '{command}'", "Debug - D�but Orion");

            // Search for Orion window
            if (hwndUOClient == IntPtr.Zero || !IsWindow(hwndUOClient))
            {
                //System.Windows.MessageBox.Show("Recherche de la fen�tre Orion...", "Debug - Recherche Orion");

                hwndUOClient = IntPtr.Zero;
                EnumWindows((hWnd, lParam) =>
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(hWnd, sb, 256);
                    string title = sb.ToString();

                    if (title.Contains("Orion") || title.Contains("Ultima Online") || title.Contains("UOSA") ||
                        (!string.IsNullOrEmpty(_uoTitle) && title.Contains(_uoTitle)))
                    {
                        hwndUOClient = hWnd;
                        //System.Windows.MessageBox.Show($"Fen�tre Orion trouv�e! Titre: '{title}'\nHandle: {hWnd}", "Debug - Orion trouv�e");
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);
            }

            if (hwndUOClient != IntPtr.Zero)
            {
                bool focusResult = ForceSetForegroundWindow(hwndUOClient);
                //System.Windows.MessageBox.Show($"Focus Orion r�sultat: {focusResult}", "Debug - Focus Orion");

                await Task.Delay(100);
                // Essayer la m�thode directe
                //System.Windows.MessageBox.Show("Tentative DirectMessage...", "Debug - M�thode Directe");
                return await SendToOrionDirectMessage(command);
            }
            else
            {
                //System.Windows.MessageBox.Show("Aucune fen�tre Orion trouv�e!", "Debug - Erreur Orion");
            }

            return false;
        }
    }
}
