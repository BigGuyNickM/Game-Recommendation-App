using System;
using System.Collections.Generic;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Repositories;

namespace Game_Recommendation.Cli.Managers
{
    public class AccountManager : BaseMenu
    {
        private readonly User currentUser;
        private readonly GenreManager genreManager;
        private readonly GenreRepository genreRepo;

        public AccountManager(User user)
        {
            currentUser = user;
            genreManager = new GenreManager();
            genreRepo = new GenreRepository();
        }

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("MANAGE ACCOUNT");
            ConsoleHelper.PrintColored($"{currentUser.Username}\n", ColorScheme.Highlight);
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
            List<Genre> preferredGenres = genreRepo.GetUserPreferredGenres(currentUser.Id);

            ConsoleHelper.PrintHeader("VIEW PROFILE");

            ConsoleHelper.PrintLine(
                ("Username:  ", ColorScheme.Muted),
                (currentUser.Username, ColorScheme.Highlight)
            );
            ConsoleHelper.PrintLine(
                ("Email:     ", ColorScheme.Muted),
                (currentUser.Email, ColorScheme.Highlight)
            );
            ConsoleHelper.PrintLine(
                ("Joined:    ", ColorScheme.Muted),
                (currentUser.CreatedAt.ToString("MMMM dd, yyyy"), ColorScheme.Highlight)
            );

            Console.WriteLine();
            ConsoleHelper.PrintColored("Preferred Genres:", ColorScheme.Muted);

            if (preferredGenres.Count == 0)
                ConsoleHelper.PrintColored("  None set.", ColorScheme.Default);
            else
                foreach (Genre genre in preferredGenres)
                    ConsoleHelper.PrintColored($"  • {genre.GenreName}", ColorScheme.Default);

            Console.WriteLine();
            ConsoleHelper.PrintColored("Played Games:", ColorScheme.Muted);
            ConsoleHelper.PrintColored("  (Work in Progress)", ColorScheme.Default);

            InputHelper.WaitForKey();
        }

        private void _ManagePlayedGames()
        {
            ConsoleHelper.PrintHeader("MANAGE PLAYED GAMES");
            ConsoleHelper.PrintColored("(Work in Progress)", ColorScheme.Muted);
            InputHelper.WaitForKey();
        }
    }
}