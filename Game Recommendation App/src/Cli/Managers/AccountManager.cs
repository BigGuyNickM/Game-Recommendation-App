using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game_Recommendation.Cli.Managers
{
    public class AccountManager : BaseMenu
    {
        private readonly User currentUser;
        private readonly GenreManager genreManager;
        private readonly GenreRepository genreRepo;

        public AccountManager(User user, GenreManager genreManager, GenreRepository genreRepo)
        {
            currentUser = user;
            this.genreManager = genreManager;
            this.genreRepo = genreRepo;
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("MANAGE ACCOUNT");
            ConsoleHelper.PrintColored($"{currentUser.Username}\n", AppConfig.Highlight);
            ConsoleHelper.PrintOptions(
                ("1", "Manage Preferred Genres"),
                ("2", "Manage Played Games"),
                ("3", "View Profile"),
                ("0", "Back")
            );
        }

        protected override void _HandleChoice(string choice)
        {
            switch (choice)
            {
                case "1": genreManager.ManagePreferences(currentUser.Id); break;
                case "2": _ManagePlayedGames(); break;
                case "3": _ViewProfile(); break;
                case "0": _Exit(); break;
            }
        }

        private void _ViewProfile()
        {
            ConsoleHelper.PrintHeader("VIEW PROFILE");

            List<Genre> preferredGenres = genreRepo.GetUserPreferredGenres(currentUser.Id);
            string genreList = preferredGenres.Count == 0
                ? "  None set."
                : "\n" + string.Join("\n", preferredGenres.Select(g => $"  • {g.GenreName}"));

            ConsoleHelper.PrintColored(
                ("Username:  ", AppConfig.Muted),
                (currentUser.Username, AppConfig.Highlight)
            ); // USERNAME
            ConsoleHelper.PrintColored(
                ("Email:     ", AppConfig.Muted),
                (currentUser.Email, AppConfig.Highlight)
            ); // EMAIL
            ConsoleHelper.PrintColored(
                ("Joined:    ", AppConfig.Muted),
                (currentUser.CreatedAt.ToString("MMMM dd, yyyy"), AppConfig.Highlight)
            ); // JOINED DATE

            

            ConsoleHelper.PrintColored(
                ("\nPreferred Genres:", AppConfig.Muted),
                (genreList, AppConfig.Default)
            ); // PREFERRED GENRES

            ConsoleHelper.PrintColored("\nPlayed Games:", AppConfig.Muted);
            ConsoleHelper.PrintColored("  (Work in Progress)", AppConfig.Default);

            InputHelper.WaitForKey("Press any key to go back...");
        }

        private void _ManagePlayedGames()
        {
            ConsoleHelper.PrintHeader("MANAGE PLAYED GAMES");
            ConsoleHelper.PrintColored("(Work in Progress)", AppConfig.Muted);
            InputHelper.WaitForKey("Press any key to go back...");
        }
    }
}