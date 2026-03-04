using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game_Recommendation.Cli.Managers.Account
{
    public class AccountManager : BaseMenu
    {
        private readonly User _currentUser;
        private readonly GenreManager _genreManager;
        private readonly GenreRepository _genreRepo;
        private readonly UserGameRepository _userGameRepo;

        public AccountManager(User user, GenreManager genreManager, GenreRepository genreRepo, UserGameRepository userGameRepo)
        {
            _currentUser = user;
            _genreManager = genreManager;
            _genreRepo = genreRepo;
            _userGameRepo = userGameRepo;
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("MANAGE ACCOUNT");
            ConsoleHelper.PrintOptions(
                ("1", "Manage Preferred Genres"),
                ("2", "View Profile"),
                ("0", "Back")
            );
        }

        protected override void _HandleChoice(string choice)
        {
            switch (choice)
            {
                case "1": _genreManager.ManagePreferences(_currentUser.Id); break;
                case "2": _ViewProfile(); break;
                case "0": _Exit(); break;
            }
        }

        // --- Private helpers ---

        private void _ViewProfile()
        {
            while (true)
            {
                List<UserGame> playedGames = _userGameRepo.GetPlayedGames(_currentUser.Id);
                List<UserWishlist> wishlistedGames = _userGameRepo.GetWishlistedGames(_currentUser.Id);
                List<Genre> preferredGenres = _genreRepo.GetUserPreferredGenres(_currentUser.Id);

                ConsoleHelper.PrintHeader("VIEW PROFILE");

                ConsoleHelper.PrintColored(("Username:  ", AppConfig.Muted), (_currentUser.Username, AppConfig.Highlight));
                ConsoleHelper.PrintColored(("Email:     ", AppConfig.Muted), (_currentUser.Email, AppConfig.Highlight));
                ConsoleHelper.PrintColored(("Joined:    ", AppConfig.Muted), (_currentUser.CreatedAt.ToString("MMMM dd, yyyy") + "\n", AppConfig.Highlight));

                _PrintProfileSection("Preferred Genres", preferredGenres.Select(g => g.GenreName).ToList());
                _PrintProfileSection("Played Games", playedGames.Select(g => $"{g.Title} — {g.RatingName ?? "Unrated"} — {g.HoursPlayed}h").ToList());
                _PrintProfileSection("Wishlisted Games", wishlistedGames.Select(g => g.Title).ToList());

                bool hasMoreGenres = preferredGenres.Count > AppConfig.ProfilePreviewCount;
                bool hasMorePlayed = playedGames.Count > AppConfig.ProfilePreviewCount;
                bool hasMoreWishlisted = wishlistedGames.Count > AppConfig.ProfilePreviewCount;

                Console.WriteLine();
                if (hasMoreGenres) ConsoleHelper.PrintOptions("1", $"View All Preferred Genres ({preferredGenres.Count})");
                if (hasMorePlayed) ConsoleHelper.PrintOptions("2", $"View All Played Games ({playedGames.Count})");
                if (hasMoreWishlisted) ConsoleHelper.PrintOptions("3", $"View All Wishlisted Games ({wishlistedGames.Count})");
                ConsoleHelper.PrintOptions("0", "Back");

                string input = InputHelper.GetInput();

                if (input == "0") return;
                if (input == "1" && hasMoreGenres)
                    _ViewAllGames("PREFERRED GENRES", preferredGenres.Select(g => g.GenreName).ToList());
                if (input == "2" && hasMorePlayed)
                    _ViewAllGames("PLAYED GAMES", playedGames.Select(g => $"{g.Title} — {g.RatingName ?? "Unrated"} — {g.HoursPlayed}h").ToList());
                if (input == "3" && hasMoreWishlisted)
                    _ViewAllGames("WISHLISTED GAMES", wishlistedGames.Select(g => g.Title).ToList());
            }
        }

        private void _PrintProfileSection(string title, List<string> items)
        {
            ConsoleHelper.PrintColored($"\n{title}:", AppConfig.Muted);

            if (items.Count == 0)
            {
                ConsoleHelper.PrintColored("  None.", AppConfig.Default);
                return;
            }

            foreach (string item in items.Take(AppConfig.ProfilePreviewCount))
                ConsoleHelper.PrintColored($"  • {item}", AppConfig.Default);

            if (items.Count > AppConfig.ProfilePreviewCount)
                ConsoleHelper.PrintColored($"  ... and {items.Count - AppConfig.ProfilePreviewCount} more", AppConfig.Muted);
        }

        private void _ViewAllGames(string header, List<string> items)
        {
            int page = 0;
            int totalPages = (int)Math.Ceiling(items.Count / (double)AppConfig.DefaultPageSize);

            while (true)
            {
                ConsoleHelper.PrintHeader(header);
                ConsoleHelper.PrintColored($"{items.Count} item(s):\n", AppConfig.Muted);

                int start = page * AppConfig.DefaultPageSize;
                foreach (string item in items.Skip(start).Take(AppConfig.DefaultPageSize))
                    ConsoleHelper.PrintColored(("  • ", AppConfig.Muted), (item, AppConfig.Highlight));

                ConsoleHelper.PrintColored($"\nPage {page + 1} of {totalPages}\n", AppConfig.Muted);
                if (page > 0) ConsoleHelper.PrintOptions("A", "Previous Page");
                if (page < totalPages - 1) ConsoleHelper.PrintOptions("D", "Next Page");
                ConsoleHelper.PrintOptions("0", "Back");

                string input = InputHelper.GetInput();

                if (input == "0") return;
                if (input.ToUpper() == "D" && page < totalPages - 1) { page++; continue; }
                if (input.ToUpper() == "A" && page > 0) { page--; continue; }
            }
        }
    }
}