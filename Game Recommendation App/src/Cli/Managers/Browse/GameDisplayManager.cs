using System;
using System.Collections.Generic;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Database.Repositories;


namespace Game_Recommendation.Cli.Managers.Browse
{
    public class GameDisplayManager
    {
        private readonly User _currentUser;
        private readonly UserGameRepository _userGameRepo;

        public GameDisplayManager(User currentUser, UserGameRepository userGameRepo)
        {
            _currentUser = currentUser;
            _userGameRepo = userGameRepo;
        }

        public void ShowResults(List<Game> results)
        {
            if (results.Count == 0)
            {
                _ShowResultsPage(results, 0, 1);
                InputHelper.WaitForKey();
                return;
            }

            int page = 0;
            int totalPages = (int)Math.Ceiling(results.Count / (double)AppConfig.DefaultPageSize);

            while (true)
            {
                _ShowResultsPage(results, page, totalPages);

                string input = InputHelper.GetInput();

                int start = page * AppConfig.DefaultPageSize;
                int end = Math.Min(start + AppConfig.DefaultPageSize, results.Count);

                if (input == "0") return;
                if (input.ToUpper() == "D" && page < totalPages - 1) { page++; continue; }
                if (input.ToUpper() == "A" && page > 0) { page--; continue; }
                if (!int.TryParse(input, out int number) || number < start + 1 || number > end) continue;

                ShowGameDetail(results[number - 1]);
            }
        }

        public void ShowGameDetail(Game game)
        {
            while (true)
            {
                bool isPlayed = _userGameRepo.IsGamePlayed(_currentUser.Id, game.Id);
                bool isWishlisted = _userGameRepo.IsGameWishlisted(_currentUser.Id, game.Id);

                ConsoleHelper.PrintHeader(game.Title);

                ConsoleHelper.PrintColored(
                    ("Publisher:  ", AppConfig.Muted),
                    (game.Publisher, AppConfig.Highlight)
                );
                ConsoleHelper.PrintColored(
                    ("Genres:     ", AppConfig.Muted),
                    (string.Join(", ", game.Genres), AppConfig.Highlight)
                );

                string rating = game.AvgRating.HasValue
                    ? $"★ {game.AvgRating.Value:0.00} ({game.TotalRatings} ratings)"
                    : "N/A";
                ConsoleHelper.PrintColored(
                    ("Rating:     ", AppConfig.Muted),
                    (rating + "\n", AppConfig.Highlight)
                );

                ConsoleHelper.PrintOptions(
                    ("1", isPlayed ? "Remove from Played Games" : "Add to Played Games"),
                    ("2", isWishlisted ? "Remove from Wishlist" : "Add to Wishlist"),
                    ("0", "Back")
                );

                string input = InputHelper.GetInput();

                if (input == "0") return;
                if (input == "1") _HandlePlayedToggle(game, isPlayed);
                if (input == "2") _HandleWishlistToggle(game, isWishlisted);
            }
        }

        // --- Private helpers ---

        private void _ShowResultsPage(List<Game> results, int page, int totalPages)
        {
            ConsoleHelper.PrintHeader("SEARCH RESULTS");

            if (results.Count == 0)
            {
                ConsoleHelper.PrintColored("No games found matching your search.", AppConfig.Muted);
                return;
            }

            ConsoleHelper.PrintColored($"{results.Count} game(s) found:\n", AppConfig.Muted);
            ConsoleHelper.PrintGameGrid(results, page, totalPages);
        }

        private void _HandleWishlistToggle(Game game, bool isWishlisted)
        {
            if (isWishlisted)
            {
                _userGameRepo.RemoveFromWishlist(_currentUser.Id, game.Id);
                ConsoleHelper.PrintColored($"\n{game.Title} removed from wishlist.", AppConfig.Muted);
            }
            else
            {
                _userGameRepo.AddToWishlist(_currentUser.Id, game.Id);
                ConsoleHelper.PrintColored($"\n{game.Title} added to wishlist!", AppConfig.Success);
            }
            InputHelper.WaitForKey();
        }

        private void _HandlePlayedToggle(Game game, bool isPlayed)
        {
            if (isPlayed)
            {
                _userGameRepo.RemovePlayedGame(_currentUser.Id, game.Id);
                ConsoleHelper.PrintColored($"\n{game.Title} removed from played games.", AppConfig.Muted);
                InputHelper.WaitForKey();
                return;
            }

            int ratingId = _GetRating();
            if (ratingId == -1) return;

            int hoursPlayed = _GetHoursPlayed();
            if (hoursPlayed == -1) return;

            _userGameRepo.AddPlayedGame(_currentUser.Id, game.Id, ratingId, hoursPlayed);
            ConsoleHelper.PrintColored($"\n{game.Title} added to played games!", AppConfig.Success);
            InputHelper.WaitForKey();
        }

        private int _GetRating()
        {
            List<int> ratings = _userGameRepo.GetRatings();

            while (true)
            {
                ConsoleHelper.PrintHeader("RATE GAME");
                ConsoleHelper.PrintOptions(
                    ("1", "Disliked"),
                    ("2", "Liked"),
                    ("3", "Loved"),
                    ("0", "Back")
                );

                string input = InputHelper.GetInput();

                if (input == "0") return -1;
                if (input == "1") return ratings[0];
                if (input == "2") return ratings[1];
                if (input == "3") return ratings[2];
            }
        }

        private int _GetHoursPlayed()
        {
            string error = null;

            while (true)
            {
                ConsoleHelper.PrintHeader("HOURS PLAYED");
                ConsoleHelper.PrintOptions("0", "Back");
                ConsoleHelper.PrintColored("How many hours have you played?\n", AppConfig.Default);

                if (error != null)
                {
                    ConsoleHelper.PrintError(error + "\n");
                    error = null;
                }

                string input = InputHelper.GetInput();

                if (input == "0") return -1;
                if (!int.TryParse(input, out int hours) || hours < 0)
                {
                    error = "Please enter a valid number of hours.";
                    continue;
                }

                return hours;
            }
        }
    }
}