using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Text;

// Todo:
// Add different color via ANSI escape codes
// Add different sizes of brushes

namespace AspectPaint
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            InitializeConsole();
            Maximize();
            string[] canvas = Enumerable.Repeat(new string(' ', Console.WindowWidth), Console.WindowHeight).ToArray();
            StringBuilder newLine;
            bool canMaximize = true;
            while (true)
            {
                Console.CursorVisible = false;
                if (Control.MouseButtons == MouseButtons.Left)
                {
                    try
                    {
                        object[] p = Relative.CharacterPosition(22, 23);
                        int mouseX = int.Parse(p[0].ToString());
                        int mouseY = int.Parse(p[1].ToString());
                        newLine = new(canvas[mouseY]);
                        newLine[mouseX] = '#';
                        canvas[mouseY] = newLine.ToString();
                        canMaximize = true;
                    }
                    catch
                    {
                        // User probably switched windows, don't maximize the foreground window
                        canMaximize = false;
                    }
                }
                if (canMaximize) { Maximize(); }
                Console.SetCursorPosition(0, 0);
                Console.Write(string.Join('\n', canvas));
            }
            Console.CursorVisible = true;
        }

        private const uint ENABLE_QUICK_EDIT = 0x0040;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        private const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static bool InitializeConsole()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                return false;
            }
            consoleMode &= ~ENABLE_QUICK_EDIT;
            consoleMode |= ENABLE_MOUSE_INPUT;
            consoleMode |= ENABLE_EXTENDED_FLAGS;
            return SetConsoleMode(consoleHandle, consoleMode);
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private static void Maximize()
        {
            // Code to maximize the console window
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            ShowWindow(GetConsoleWindow(), 3);
        }
    }
}