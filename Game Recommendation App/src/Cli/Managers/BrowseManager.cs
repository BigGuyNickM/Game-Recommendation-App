using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Repositories;

namespace Game_Recommendation.Cli.Managers
{
    public class BrowseManager : BaseMenu
    {
        private readonly User _currentUser;
        private readonly SearchManager _searchManager;

        public BrowseManager(User currentUser, SearchManager searchManager)
        {
            _currentUser = currentUser;
            _searchManager = searchManager;
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("BROWSE GAMES");
            ConsoleHelper.PrintColored($"Welcome, {_currentUser.Username}!\n", AppConfig.Success);
            ConsoleHelper.PrintOptions(
                ("1", "Search Games"),
                ("2", "Top Rated Games"),
                ("0", "Back")
            );
        }

        protected override void _HandleChoice(string choice)
        {
            switch (choice)
            {
                case "1": _searchManager.Run(); break;
                case "2": _TopRatedGames(); break;
                case "0": _Exit(); break;
            }
        }

        private void _TopRatedGames()
        {
            ConsoleHelper.PrintHeader("TOP RATED GAMES");
            ConsoleHelper.PrintColored("(Work in Progress)", AppConfig.Muted);
            InputHelper.WaitForKey();
        }
    }
}