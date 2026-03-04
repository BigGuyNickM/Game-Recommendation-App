using Game_Recommendation.Cli.Config;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Game_Recommendation.Cli.Utils
{
    internal static class InputHelper
    {
        #region Public

        public static string GetInput(string prompt = "")
        {
            PrintPrompt(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        // Keeps asking until user gives a clear y/n answer
        public static bool Confirm(string prompt)
        {
            while (true)
            {
                // Strip anything that isn't a letter so "y!" or "yes." still works
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

        // Masks input as asterisks, optionally shows live password strength
        public static string GetMaskedInput(string prompt = "", bool showStrength = false)
        {
            PrintPrompt(prompt, showStrength);
            var input = new StringBuilder();
            int strengthRow = showStrength ? Console.CursorTop - 2 : -1;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }

                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b"); // erase last asterisk
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input.Append(key.KeyChar);
                    Console.Write("*");
                }

                if (showStrength) UpdateStrengthDisplay(input.ToString(), strengthRow);
            }

            return input.ToString();
        }

        #endregion

        #region Helpers

        private static void PrintPrompt(string prompt, bool showStrength = false)
        {
            if (!string.IsNullOrEmpty(prompt)) Console.WriteLine($"{prompt}\n");
            if (showStrength) ConsoleHelper.PrintPasswordStrength("None\n", 0);
            ConsoleHelper.PrintColored("> ", AppConfig.Input, newLine: false);
        }

        // Saves cursor position, rewrites the strength line, then jumps back
        private static void UpdateStrengthDisplay(string input, int strengthRow)
        {
            var (label, score) = ValidationHelper.GetPasswordStrength(input);
            int col = Console.CursorLeft;
            int row = Console.CursorTop;

            Console.SetCursorPosition(0, strengthRow);
            Console.Write(new string(' ', Console.WindowWidth)); // clear old strength text
            Console.SetCursorPosition(0, strengthRow);
            ConsoleHelper.PrintPasswordStrength(label, score);

            Console.SetCursorPosition(col, row);
        }

        #endregion
    }
}