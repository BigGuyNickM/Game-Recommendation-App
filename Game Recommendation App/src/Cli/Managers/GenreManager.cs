using System;
using System.Collections.Generic;
using System.Linq;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Repositories;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli.Managers
{
    public class GenreManager
    {
        private readonly GenreRepository genreRepo;

        public GenreManager(GenreRepository genreRepo)
        {
            this.genreRepo = genreRepo;
        }

        // For signup: select preferences from scratch
        public void SelectPreferences(int userId)
        {
            List<Genre> allGenres = genreRepo.GetAllGenres();
            List<int> selectedIds = new();
            string error = null;

            while (true)
            {
                _PrintGenreMenu("SELECT YOUR FAVORITE GENRES", $"Pick at least {AppConfig.MinGenreSelections} genres you enjoy", allGenres, selectedIds, error);
                error = null;
                string input = InputHelper.GetInput();

                if (input == "0" && selectedIds.Count < AppConfig.MinGenreSelections)
                {
                    error = $"Please select at least {AppConfig.MinGenreSelections} genres before finishing.";
                    continue;
                }
                if (input == "0") break;

                error = _ToggleGenreInList(allGenres, selectedIds, input);
            }

            genreRepo.SaveUserPreferences(userId, selectedIds);
        }

        // For menu: manage existing preferences
        public void ManagePreferences(int userId)
        {
            string error = null;
            List<Genre> allGenres = genreRepo.GetAllGenres();

            while (true)
            {
                List<int> userGenreIds = genreRepo.GetUserPreferredGenres(userId).Select(g => g.Id).ToList();

                _PrintGenreMenu("MANAGE PREFERRED GENRES", $"You have {userGenreIds.Count} genre(s) selected (minimum {AppConfig.MinGenreSelections} required)", allGenres, userGenreIds, error);
                error = null;

                string input = InputHelper.GetInput();
                if (input == "0") break;

                error = _ToggleGenreInDatabase(allGenres, userGenreIds, input, userId);
            }
        }

        // --- Private helpers ---

        private void _PrintGenreMenu(string title, string subtitle, List<Genre> allGenres, List<int> selectedIds, string error)
        {
            ConsoleHelper.PrintHeader(title);
            Console.WriteLine(subtitle + "\n");
            _PrintGenreGrid(allGenres, selectedIds);
            Console.WriteLine($"\nSelected: {selectedIds.Count}\n");
            ConsoleHelper.PrintOptions(("0", "Done"));

            if (error != null)
                ConsoleHelper.PrintError("\n" + error);

            Console.WriteLine();
        }

        private void _PrintGenreGrid(List<Genre> allGenres, List<int> selectedIds)
        {
            int maxNameLength = allGenres.Max(g => g.GenreName.Length);
            int numberWidth = allGenres.Count.ToString().Length;

            Action[] items = allGenres.Select((genre, i) =>
            {
                bool isSelected = selectedIds.Contains(genre.Id);
                ConsoleColor checkColor = isSelected ? AppConfig.Success : AppConfig.Default;
                ConsoleColor nameColor = isSelected ? AppConfig.Highlight : AppConfig.Default;
                string checkbox = isSelected ? "(X) " : "( ) ";

                return (Action)(() =>
                {
                    ConsoleHelper.PrintColored($"[{i + 1}] ".PadRight(numberWidth + 3), AppConfig.Input, newLine: false);
                    ConsoleHelper.PrintColored(checkbox, checkColor, newLine: false);
                    ConsoleHelper.PrintColored(genre.GenreName.PadRight(maxNameLength), nameColor, newLine: false);
                });
            }).ToArray();

            ConsoleHelper.PrintOptions(items, AppConfig.DefaultGridColumns);
        }

        private string _ToggleGenreInList(List<Genre> allGenres, List<int> selectedIds, string input)
        {
            var (genre, error) = _ParseGenreInput(allGenres, input);
            if (genre == null) return error;

            if (selectedIds.Contains(genre.Id))
                selectedIds.Remove(genre.Id);
            else
                selectedIds.Add(genre.Id);

            return null;
        }

        private string _ToggleGenreInDatabase(List<Genre> allGenres, List<int> userGenreIds, string input, int userId)
        {
            var (genre, error) = _ParseGenreInput(allGenres, input);
            if (genre == null) return error;

            if (userGenreIds.Contains(genre.Id) && userGenreIds.Count <= AppConfig.MinGenreSelections)
                return $"Cannot remove — you must have at least {AppConfig.MinGenreSelections} genres selected.";

            if (userGenreIds.Contains(genre.Id))
                genreRepo.RemoveUserPreference(userId, genre.Id);
            else
                genreRepo.AddUserPreference(userId, genre.Id);

            return null;
        }

        private (Genre genre, string error) _ParseGenreInput(List<Genre> allGenres, string input)
        {
            if (!int.TryParse(input, out int number) || number < 1 || number > allGenres.Count)
                return (null, $"Invalid input. Enter a number between 1 and {allGenres.Count}.");

            return (allGenres[number - 1], null);
        }
    }
}