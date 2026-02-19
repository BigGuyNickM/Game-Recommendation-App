using System;
using System.Collections.Generic;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli
{
    public class MenuManager
    {
        private bool isRunning;
        private Dictionary<string, Action> menuActions;
        private User currentUser;

        public MenuManager(User user)
        {
            isRunning = true;
            currentUser = user;

            menuActions = new Dictionary<string, Action>
            {
                { "1", SearchGames },
                { "2", GenreRecommendations },
                { "3", PlayedGamesRecommendations },
                { "4", ManagePreferredGenres },
                { "5", ManagePlayedGames },
                { "6", ViewProfile },
                { "0", Exit }
            };
        }

        // Main menu loop to display menu and handle user input
        public void Run()
        {
            while (isRunning)
            {
                ShowMenu();
                string choice = InputHelper.GetInput("Your choice: ");
                Console.Clear();
                HandleChoice(choice);
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("\n=== GAME RECOMMENDATION SYSTEM ===");
            Console.WriteLine($"Logged in as: {currentUser.Username}\n");

            Console.WriteLine("BROWSE GAMES");
            Console.WriteLine("1. Search games");
            Console.WriteLine("2. Genre-based recommendations");
            Console.WriteLine("3. Recommendations from your play history");

            Console.WriteLine("\nUSER PROFILE");
            Console.WriteLine("4. Manage preferred genres");
            Console.WriteLine("5. Manage played games");
            Console.WriteLine("6. View profile");

            Console.WriteLine("\n0. Exit");
            Console.WriteLine();
        }

        private void HandleChoice(string choice)
        {
            if (menuActions.ContainsKey(choice))
            {
                menuActions[choice]();
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }

            if (isRunning)
                ConsoleHelper.WaitForKey();
        }

        // BROWSE GAMES OPTIONS

        private void SearchGames()
        {
            Console.WriteLine("=== SEARCH GAMES ===\n");
            Console.WriteLine("(Work in Progress)");
        }

        private void GenreRecommendations()
        {
            Console.WriteLine("=== GENRE-BASED RECOMMENDATIONS ===\n");
            Console.WriteLine("(Work in Progress)");
        }

        private void PlayedGamesRecommendations()
        {
            Console.WriteLine("=== RECOMMENDATIONS BASED ON PLAYED GAMES ===\n");
            Console.WriteLine("(Work in Progress)");
        }

        // USER PROFILE OPTIONS

        private void ManagePreferredGenres()
        {
            GenreManager genreManager = new GenreManager();
            genreManager.ManagePreferences(currentUser.Id);
        }

        private void ManagePlayedGames()
        {
            Console.WriteLine("=== MANAGE PLAYED GAMES ===\n");
            Console.WriteLine("(Work in Progress)");
        }

        private void ViewProfile()
        {
            Console.WriteLine("=== VIEW PROFILE ===\n");
            Console.WriteLine("(Work in Progress)");
        }

        private void Exit()
        {
            isRunning = false;
            Console.WriteLine("Goodbye!");
        }
    }
}