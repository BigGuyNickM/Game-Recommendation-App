using Game_Recommendation.Cli.Config;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Game_Recommendation.Cli.Utils
{
    internal static class InputHelper
    {
        public static string GetInput(string prompt = "")
        {
            PrintPrompt(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static bool Confirm(string prompt)
        {
            while (true)
            {
                string input = Regex.Replace(GetInput(prompt).ToLower(), @"[^a-z]", "");
                if (input is "y" or "yes" or "yep" or "yeah" or "yea") return true;
                if (input is "n" or "no" or "nah") return false;
                ConsoleHelper.PrintError("Please enter y or n.");
            }
        }

        public static void WaitForKey(string message = "\nPress any key to continue...")
        {
            Console.WriteLine(message);
            Console.ReadKey(intercept: true);
        }

        public static string GetMaskedInput(string prompt = "", bool showStrength = false)
        {
            PrintPrompt(prompt, showStrength);
            StringBuilder input = new();
            int strengthRow = showStrength ? Console.CursorTop - 2 : -1;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b");
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
                Console.WriteLine($"{prompt}\n");

            if (showStrength)
            {
                ConsoleHelper.PrintPasswordStrength("None\n", 0);
            }

            ConsoleHelper.PrintColored("> ", AppConfig.Input, newLine: false);
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