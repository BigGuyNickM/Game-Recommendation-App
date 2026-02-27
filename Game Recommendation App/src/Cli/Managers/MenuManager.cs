using System;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli.Managers
{
    public class MenuManager : BaseMenu
    {
        private readonly User currentUser;
        private readonly AccountManager accountManager;

        public MenuManager(User user)
        {
            currentUser = user;
            accountManager = new AccountManager(user);
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("GAME RECOMMENDATION SYSTEM");
            ConsoleHelper.PrintColored($"Welcome, {currentUser.Username}\n", ColorScheme.Muted);
            ConsoleHelper.PrintOptions(
                ("1", "Browse Games"),
                ("2", "Manage Account"),
                ("0", "Exit")
            );
        }

        protected override void _HandleChoice(string choice)
        {
            switch (choice)
            {
                case "1": _BrowseGames(); break;
                case "2": accountManager.Run(); break;
                case "0": _Exit(); break;
            }
        }

        private void _BrowseGames()
        {
            ConsoleHelper.PrintHeader("BROWSE GAMES");
            ConsoleHelper.PrintColored("(Work in Progress)", ColorScheme.Muted);
            InputHelper.WaitForKey();
        }
    }
}