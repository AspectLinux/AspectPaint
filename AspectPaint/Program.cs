using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

// Todo:
// Add different color via ANSI escape codes
// Add different sizes of brushes
// Can skip pixels if you move the mouse fast enough, maybe calculate a line to draw on top of those spots?

// This program also doesn't work in Windows Terminal for some reason
// But it doesn't matter since this OS is based of of Arch anyway.

namespace AspectPaint
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            char currentColorKey = '7';
            static List<string> GenerateCircle(int radius, int lineWidth, double xScale, char colorKey)
            {
                List<string> circle = new();
                double hUnitsPerChar = 1 / xScale;
                double hChars = ((2 * radius) + lineWidth) / hUnitsPerChar;
                double vChars = (2 * radius) + lineWidth;
                double dist;
                double lineWidth_2 = (double)lineWidth / 2;
                double center = radius + lineWidth_2;
                StringBuilder circleLine = new("");
                for (int j = 0; j <= vChars - 1; j++)
                {
                    double y = j + 0.5;
                    circleLine.Clear();
                    for (int i = 0; i <= hChars - 1; i++)
                    {
                        double x = (i + 0.5) * hUnitsPerChar;
                        dist = Math.Sqrt(
                            ((x - center) * (x - center)) +
                            ((y - center) * (y - center)));
                        if (dist < radius + lineWidth_2)
                        {
                            circleLine.Append(colorKey);
                        }
                        else
                        {
                            circleLine.Append(" ");
                        }
                    }
                    circle.Add(circleLine.ToString());
                }
                return circle;
            }
            InitializeConsole();
            Maximize();
            string[] canvas = Enumerable.Repeat(new string(' ', Console.WindowWidth), Console.WindowHeight).ToArray();
            string[] canvasHighlight = Enumerable.Repeat(new string(' ', Console.WindowWidth), Console.WindowHeight).ToArray();
            StringBuilder newLine;
            bool canMaximize = true;
            Dictionary<char, string> colors = new()
            {
                { '1', "\u001b[41m" },
                { '2', "\u001b[42m" },
                { '3', "\u001b[43m" },
                { '4', "\u001b[44m" },
                { '5', "\u001b[45m" },
                { '6', "\u001b[46m" },
                { '7', "\u001b[47m" },
            };
            int brushSize = 1;
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    string key = Console.ReadKey().Key.ToString().ToLower();
                    char pColorKey = currentColorKey;
                    try
                    {
                        currentColorKey = key[1];
                        string testForExeption = colors[currentColorKey];
                    }
                    catch
                    {
                        if (key == "uparrow")
                        {
                            brushSize++;
                        }
                        else if (key == "downarrow" && brushSize > 1)
                        {
                            brushSize--;
                        }
                        currentColorKey = pColorKey;
                    }
                }
                Console.CursorVisible = false;
                if (Control.MouseButtons == MouseButtons.Left)
                {
                    try
                    {
                        object[] p = Relative.CharacterPosition(22, 23);
                        int mouseX = int.Parse(p[0].ToString());
                        int mouseY = int.Parse(p[1].ToString());
                        List<string> circle = new();
                        if (brushSize > 3)
                        {
                            circle = GenerateCircle(brushSize - 3, 1, 2, currentColorKey);
                        }
                        else
                        {
                            switch (brushSize)
                            {
                                case 1:
                                    circle = new List<string> { currentColorKey.ToString() };
                                    break;
                                case 2:
                                    circle = new List<string> { currentColorKey.ToString() + currentColorKey.ToString(), currentColorKey.ToString() + currentColorKey.ToString() };
                                    break;
                                case 3:
                                    circle = new List<string> { currentColorKey.ToString() + currentColorKey.ToString() + currentColorKey.ToString(), currentColorKey.ToString() + currentColorKey.ToString() + currentColorKey.ToString(), currentColorKey.ToString() + currentColorKey.ToString() + currentColorKey.ToString() };
                                    break;
                            }
                        }
                        int l = 0;
                        for (int i = mouseY - (int)Math.Floor((double)circle.Count / 2); i < circle.Count + (mouseY - (int)Math.Floor((double)circle.Count / 2)); i++, l++)
                        {
                            double circleLength = circle[l].Length;
                            newLine = new(canvasHighlight[i]);
                            int k = 0;
                            for (int j = mouseX - (int)Math.Floor(circleLength / 2); j < circle[l].Length + (mouseX - (int)Math.Floor(circleLength / 2)); j++, k++)
                            {
                                if (circle[l][k] != ' ')
                                {
                                    newLine[j] = currentColorKey;
                                }
                            }
                            canvasHighlight[i] = newLine.ToString();
                        }
                        canMaximize = true;
                    }
                    catch
                    {
                        // User probably went onto another window, do nothing
                    }
                }
                if (canMaximize) { Maximize(); }
                Console.SetCursorPosition(0, 0);
                for (int j = 0; j < canvas.Length; j++)
                {
                    string line = canvasHighlight[j];
                    for (int i = line.Length - 1; i >= 0; i--)
                    {
                        if (line[i] != ' ')
                        {
                            newLine = new(line);
                            newLine[i] = '#';
                            string[] split = newLine.ToString().Split('#');
                            line = string.Join($"{colors[line[i]]} \u001b[0m", split);
                            canvas[j] = line;
                        }
                    }
                }
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
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void Maximize()
        {
            // Code to maximize the console window
            ShowWindow(GetConsoleWindow(), 3);
        }
    }
}