using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Repositories;
using System;
using System.Collections.Generic;

namespace Game_Recommendation.Cli.Managers
{
    public class GameDisplayManager
    {
        private readonly GameRepository _gameRepo;

        public GameDisplayManager(GameRepository gameRepo)
        {
            _gameRepo = gameRepo;
        }

        public void ShowResults(List<Game> results)
        {
            if (results.Count == 0)
            {
                ConsoleHelper.PrintHeader("SEARCH RESULTS");
                ConsoleHelper.PrintColored("No games found matching your search.", AppConfig.Muted);
                InputHelper.WaitForKey();
                return;
            }

            int page = 0;
            int totalPages = (int)Math.Ceiling(results.Count / (double)AppConfig.DefaultPageSize);

            while (true)
            {
                ConsoleHelper.PrintHeader("SEARCH RESULTS");
                ConsoleHelper.PrintColored($"{results.Count} game(s) found:\n", AppConfig.Muted);
                ConsoleHelper.PrintGameGrid(results, page, totalPages);

                string input = InputHelper.GetInput();

                if (input == "0") return;
                if (input.ToUpper() == "D" && page < totalPages - 1) { page++; continue; }
                if (input.ToUpper() == "A" && page > 0) { page--; continue; }
                if (!int.TryParse(input, out int number) || number < 1 || number > results.Count) continue;

                ShowGameDetail(results[number - 1]);
            }
        }

        public void ShowGameDetail(Game game)
        {
            ConsoleHelper.PrintHeader(game.Title);
            ConsoleHelper.PrintColored("(Work in Progress)", AppConfig.Muted);
            InputHelper.WaitForKey("\nPress any key to go back...");
        }
    }
}