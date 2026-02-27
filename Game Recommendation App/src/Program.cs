using System;
using Game_Recommendation.Cli;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Cli.Managers;
using Game_Recommendation.Models;


namespace Game_Recommendation
{
    class Program
    {
        static void Main(string[] args)
        {
            AuthManager authManager = new AuthManager();
            while (true)
            {
                ConsoleHelper.PrintHeader("GAME RECOMMENDATION SYSTEM");
                ConsoleHelper.PrintOptions(
                    ("1", "Login"),
                    ("2", "Sign Up"),
                    ("0", "Exit")
                );
                Console.WriteLine();

                string choice = InputHelper.GetInput("Please choose an option:");

                if (choice == "0")
                {
                    Console.Clear();
                    Console.WriteLine("Goodbye!");
                    return;
                }

                
                User currentUser = choice == "1" ? authManager.Login()
                 : choice == "2" ? authManager.Signup()
                 : null;

                if (currentUser == null) continue;

                if (currentUser.IsNewUser)
                {
                    GenreManager genreManager = new GenreManager();
                    genreManager.SelectPreferences(currentUser.Id);
                }
                else
                {
                    InputHelper.WaitForKey("\nPress any key to continue to main menu...");
                }

                MenuManager menu = new MenuManager(currentUser);
                menu.Run();

                Console.WriteLine("\nThank you for using the Game Recommendation System!");
                return;
            }
        }
    }
}