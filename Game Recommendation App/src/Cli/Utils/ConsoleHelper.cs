using System;
using Game_Recommendation.Cli.Config;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ConsoleHelper
    {
        public static void PrintHeader(string title)
        {
            Console.Clear();
            int width = Math.Max(AppConfig.HeaderMinWidth, title.Length + (AppConfig.HeaderPadding+2));
            int innerWidth = width - 2;
            string centeredTitle = title.PadLeft((innerWidth + title.Length) / 2).PadRight(innerWidth);
            string top = $"╔{new string('═', innerWidth)}╗";
            string middle = $"║{centeredTitle}║";
            string bottom = $"╚{new string('═', innerWidth)}╝";
            PrintColored(top + "\n" + middle + "\n" + bottom + "\n", AppConfig.Header);
        }

        // Prints a single colored segment
        public static void PrintColored(string text, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
            if (newLine) Console.WriteLine();
        }

        // Overload for multiple colored segments
        public static void PrintColored(params (string text, ConsoleColor color)[] segments)
        {
            foreach (var (text, color) in segments)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void PrintError(string message)
        {
            PrintColored(message, AppConfig.Error);
        }

        public static void PrintPasswordStrength(string label, int score)
        {
            ConsoleColor color = score switch
            {
                <= 1 => AppConfig.Error,
                <= 3 => AppConfig.Input,
                _ => AppConfig.Success,
            };
            PrintColored(("Password strength: ", AppConfig.Highlight), (label, color));
        }

        // Prints options vertically: [1] Option 1    [2] Option 2    [0] Exit
        public static void PrintOptions(params (string key, string label)[] options)
        {
            foreach (var (key, label) in options)
                PrintColored(($"[{key}]", AppConfig.Input), ($" {label}\n", AppConfig.Default));
        }
        // Overload for a single option
        public static void PrintOptions(string key, string label)
        {
            PrintColored(($"[{key}]", AppConfig.Input), ($" {label}\n", AppConfig.Default));
        }
        // Overload for printing options as a grid
        public static void PrintOptions(Action[] items, int columns = AppConfig.DefaultGridColumns)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i]();
                if ((i + 1) % columns == 0 || i == items.Length - 1)
                    Console.WriteLine();
                else
                    Console.Write("   ");
            }
        }
    }
}