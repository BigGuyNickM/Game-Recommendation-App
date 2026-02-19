using System;
using Game_Recommendation.Cli;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;

namespace Game_Recommendation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("=== GAME RECOMMENDATION SYSTEM ===\n");

            AuthManager authManager = new AuthManager();
            User currentUser = authManager.Login();

            if (currentUser == null)
            {
                ConsoleHelper.WaitForKey("\nLogin failed or cancelled. Exiting...", clearAfter: false);
                return;
            }

            ConsoleHelper.WaitForKey("\nPress any key to continue to main menu...");

            MenuManager menu = new MenuManager(currentUser);
            menu.Run();

            Console.WriteLine("\nThank you for using the Game Recommendation System!");
        }
    }
}