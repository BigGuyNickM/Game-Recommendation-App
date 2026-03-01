using Game_Recommendation.Cli;
using Game_Recommendation.Cli.Managers;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Data;
using Game_Recommendation.Models;
using Game_Recommendation.Repositories;
using System;


namespace Game_Recommendation
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionPool pool = ConnectionPool.Instance;
            UserRepository userRepo = new UserRepository(pool);
            GenreRepository genreRepo = new GenreRepository(pool);
            GameRepository gameRepo = new GameRepository(pool);
            GenreManager genreManager = new GenreManager(genreRepo);
            AuthManager authManager = new AuthManager(userRepo);

            _RunApp(authManager, genreManager, genreRepo);
        }

        private static void _RunApp(AuthManager authManager, GenreManager genreManager, GenreRepository genreRepo)
        {
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

                User currentUser = choice switch
                {
                    "1" => authManager.Login(),
                    "2" => authManager.Signup(),
                    _ => null
                };
                if (currentUser == null) continue;

                if (currentUser.IsNewUser)
                    genreManager.SelectPreferences(currentUser.Id);
                else
                    InputHelper.WaitForKey("\nPress any key to continue to main menu...");

                AccountManager accountManager = new AccountManager(currentUser, genreManager, genreRepo);
                MenuManager menu = new MenuManager(currentUser, accountManager);
                menu.Run();
                Console.WriteLine("\nThank you for using the Game Recommendation System!");
                return;
            }
        }
    }
}