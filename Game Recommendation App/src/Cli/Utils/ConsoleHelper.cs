using System;
using Game_Recommendation.Cli.Config;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ConsoleHelper
    {
        public static void PrintHeader(string title)
        {
            Console.Clear();
            PrintColored($"=== {title} ===\n", ColorScheme.Header);
        }

        // Single color full line
        public static void PrintColored(string text, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine) Console.WriteLine(text);
            else Console.Write(text);
            Console.ResetColor();
        }

        // Multiple colored segments on one line
        public static void PrintLine(params (string text, ConsoleColor color)[] segments)
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
            PrintColored(message, ColorScheme.Error);
        }

        public static void PrintPasswordStrength(string label, int score)
        {
            ConsoleColor color = score <= 1 ? ColorScheme.Error
                               : score <= 3 ? ColorScheme.Input
                               : ColorScheme.Success;

            PrintLine(("Password strength: ", ColorScheme.Highlight), (label, color));
        }

        // Renders options vertically: [1] Option 1    [2] Option 2    [0] Exit
        public static void PrintOptions(params (string key, string label)[] options)
        {
            foreach (var (key, label) in options)
            {
                PrintColored($"[{key}]", ColorScheme.Input, newLine: false);
                Console.WriteLine($" {label}");
            }
            Console.WriteLine();
        }
    }
}