using System;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli
{
    public class MenuManager
    {
        private readonly User currentUser;
        private readonly AccountManager accountManager;
        private bool isRunning;

        public MenuManager(User user)
        {
            currentUser = user;
            accountManager = new AccountManager(user);
            isRunning = true;
        }

        public void Run()
        {
            while (isRunning)
            {
                _ShowMenu();
                string choice = InputHelper.GetInput();

                switch (choice)
                {
                    case "1": _BrowseGames(); break;
                    case "2": accountManager.Run(); break;
                    case "0": _Exit(); break;
                    default:
                        break;
                }
            }
        }

        private void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("GAME RECOMMENDATION SYSTEM");
            ConsoleHelper.PrintColored($"Welcome, {currentUser.Username}\n", ColorScheme.Muted);
            ConsoleHelper.PrintOptions(
                ("1", "Browse Games"),
                ("2", "Manage Account"),
                ("0", "Exit")
            );
        }

        private void _BrowseGames()
        {
            ConsoleHelper.PrintHeader("BROWSE GAMES");
            ConsoleHelper.PrintColored("(Work in Progress)", ColorScheme.Muted);
            InputHelper.WaitForKey();
        }

        private void _Exit()
        {
            isRunning = false;
            Console.Clear();
            ConsoleHelper.PrintColored("Goodbye!", ColorScheme.Success);
        }
    }
}