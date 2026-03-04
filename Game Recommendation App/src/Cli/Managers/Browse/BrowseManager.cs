using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli.Managers.Browse
{
    public class BrowseManager : BaseMenu
    {
        private readonly SearchManager _searchManager;

        public BrowseManager(SearchManager searchManager)
        {
            _searchManager = searchManager;
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("BROWSE GAMES");
            ConsoleHelper.PrintOptions(
                ("1", "Search Games"),
                ("0", "Back")
            );
        }

        protected override void _HandleChoice(string choice)
        {
            switch (choice)
            {
                case "1": _searchManager.Run(); break;
                case "0": _Exit(); break;
            }
        }
    }
}