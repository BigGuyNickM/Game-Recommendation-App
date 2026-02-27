using System;
using System.Text;
using Game_Recommendation.Cli.Config;

namespace Game_Recommendation.Cli.Utils
{
    internal static class InputHelper
    {
        public static string GetInput(string prompt = "")
        {
            PrintPrompt(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static bool Confirm(string prompt, string yesValue = "y")
        {
            return GetInput(prompt).ToLower() == yesValue;
        }

        public static void WaitForKey(string message = "\nPress any key to continue...", bool clearAfter = true)
        {
            Console.WriteLine(message);
            Console.ReadKey(intercept: true);
            if (clearAfter)
                Console.Clear();
        }

        public static string GetMaskedInput(string prompt = "", bool showStrength = false)
        {
            PrintPrompt(prompt, showStrength);

            StringBuilder input = new StringBuilder();
            int strengthRow = showStrength ? Console.CursorTop - 2 : -1;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input.Remove(input.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input.Append(key.KeyChar);
                    Console.Write("*");
                }

                if (showStrength)
                    UpdateStrengthDisplay(input.ToString(), strengthRow);
            }

            return input.ToString();
        }

        // --- Private helpers ---

        private static void PrintPrompt(string prompt, bool showStrength = false)
        {
            if (!string.IsNullOrEmpty(prompt))
                Console.WriteLine(prompt + "\n");

            if (showStrength)
            {
                ConsoleHelper.PrintPasswordStrength("None", 0);
                Console.WriteLine();
            }

            ConsoleHelper.PrintColored("> ", ColorScheme.Input, newLine: false);
        }

        private static void UpdateStrengthDisplay(string input, int strengthRow)
        {
            var (label, score) = ValidationHelper.GetPasswordStrength(input);
            int currentCol = Console.CursorLeft;
            int currentRow = Console.CursorTop;

            Console.SetCursorPosition(0, strengthRow);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, strengthRow);
            ConsoleHelper.PrintPasswordStrength(label, score);

            Console.SetCursorPosition(currentCol, currentRow);
        }

    }
}