using System;

namespace Game_Recommendation.Cli.Utils
{
    internal static class InputHelper
    {
        public static string GetInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }
        public static bool Confirm(string prompt, string yesValue = "y")
        {
            string input = GetInput(prompt).ToLower();
            return input == yesValue || input == "yes";
        }
    }
}
