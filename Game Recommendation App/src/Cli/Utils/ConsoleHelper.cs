using System;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ConsoleHelper
    {
        public static void WaitForKey(string message = "\nPress any key to continue...", bool clearAfter = true)
        {
            Console.WriteLine(message);
            Console.ReadKey();
            if (clearAfter)
                Console.Clear();
        }
    }
}
