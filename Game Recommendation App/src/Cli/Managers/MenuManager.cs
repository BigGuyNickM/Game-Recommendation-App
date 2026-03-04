using System;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Cli.Managers.Browse;
using Game_Recommendation.Cli.Managers.Account;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli.Managers
{
    public class MenuManager : BaseMenu
    {
        private readonly User _currentUser;
        private readonly AccountManager _accountManager;
        private readonly BrowseManager _browseManager;

        public MenuManager(User user, AccountManager accountManager, BrowseManager browseManager)
        {
            _currentUser = user;
            _accountManager = accountManager;
            _browseManager = browseManager;
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("GAME RECOMMENDATION SYSTEM");
            ConsoleHelper.PrintColored($"Welcome, {_currentUser.Username}!\n", AppConfig.Success);
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
                case "1": _browseManager.Run(); break;
                case "2": _accountManager.Run(); break;
                case "0": _Exit(); break;
            }
        }
    }
}