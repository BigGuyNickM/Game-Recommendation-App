using System;
using System.Collections.Generic;
using System.Linq;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Repositories;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli
{
    public class GenreManager
    {
        private readonly GenreRepository genreRepo;
        private const int Columns = 3;
        private const int MinGenres = 3;

        public GenreManager()
        {
            genreRepo = new GenreRepository();
        }

        // For signup: select preferences from scratch
        public void SelectPreferences(int userId)
        {
            List<Genre> allGenres = genreRepo.GetAllGenres();
            List<int> selectedIds = new List<int>();
            string error = null;

            while (true)
            {
                _RenderScreen("SELECT YOUR FAVORITE GENRES", $"Pick at least {MinGenres} genres you enjoy", allGenres, selectedIds, error);
                error = null;

                string input = InputHelper.GetInput();
                if (input == "0")
                {
                    if (selectedIds.Count < MinGenres)
                    {
                        error = $"Please select at least {MinGenres} genres before finishing.";
                        continue;
                    }
                    break;
                }

                error = _ToggleGenreInList(allGenres, selectedIds, input);
            }

            genreRepo.SaveUserPreferences(userId, selectedIds);
        }

        // For menu: manage existing preferences
        public void ManagePreferences(int userId)
        {
            string error = null;

            while (true)
            {
                List<Genre> allGenres = genreRepo.GetAllGenres();
                List<Genre> userGenres = genreRepo.GetUserPreferredGenres(userId);
                List<int> userGenreIds = userGenres.Select(g => g.Id).ToList();

                _RenderScreen("MANAGE PREFERRED GENRES", $"You have {userGenreIds.Count} genre(s) selected (minimum {MinGenres} required)", allGenres, userGenreIds, error);
                error = null;

                string input = InputHelper.GetInput();
                if (input == "0") break;

                error = _ToggleGenreInDatabase(allGenres, userGenreIds, input, userId);
            }
        }

        // --- Private helpers ---

        private void _RenderScreen(string title, string subtitle, List<Genre> allGenres, List<int> selectedIds, string error)
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
            // Calculate cell width based on longest genre name
            int maxNameLength = allGenres.Max(g => g.GenreName.Length);
            int numberWidth = allGenres.Count.ToString().Length;
            int cellWidth = numberWidth + maxNameLength + 10; // padding for "(X) " and spacing

            for (int i = 0; i < allGenres.Count; i++)
            {
                Genre genre = allGenres[i];
                bool isSelected = selectedIds.Contains(genre.Id);
                int displayNumber = i + 1;

                // Number
                ConsoleHelper.PrintColored($"[{displayNumber}] ".PadRight(numberWidth + 3), ColorScheme.Input, newLine: false);

                // Checkbox
                if (isSelected)
                {
                    ConsoleHelper.PrintColored("(X) ", ColorScheme.Success, newLine: false);
                    ConsoleHelper.PrintColored(genre.GenreName.PadRight(maxNameLength), ColorScheme.Highlight, newLine: false);
                }
                else
                {
                    ConsoleHelper.PrintColored("( ) ", ColorScheme.Default, newLine: false);
                    ConsoleHelper.PrintColored(genre.GenreName.PadRight(maxNameLength), ColorScheme.Default, newLine: false);
                }

                // New row every 3 columns
                if ((i + 1) % Columns == 0 || i == allGenres.Count - 1)
                    Console.WriteLine();
                else
                    Console.Write("   ");
            }
        }

        private string _ToggleGenreInList(List<Genre> allGenres, List<int> selectedIds, string input)
        {
            Genre genre = _ParseGenreInput(allGenres, input, out string error);
            if (genre == null) return error;

            if (selectedIds.Contains(genre.Id))
                selectedIds.Remove(genre.Id);
            else
                selectedIds.Add(genre.Id);

            return null;
        }

        private string _ToggleGenreInDatabase(List<Genre> allGenres, List<int> userGenreIds, string input, int userId)
        {
            Genre genre = _ParseGenreInput(allGenres, input, out string error);
            if (genre == null) return error;

            if (userGenreIds.Contains(genre.Id))
            {
                if (userGenreIds.Count <= MinGenres)
                    return $"Cannot remove — you must have at least {MinGenres} genres selected.";

                genreRepo.RemoveUserPreference(userId, genre.Id);
            }
            else
            {
                genreRepo.AddUserPreference(userId, genre.Id);
            }

            return null;
        }

        // Shared input parsing and index validation
        private Genre _ParseGenreInput(List<Genre> allGenres, string input, out string error)
        {
            if (!int.TryParse(input, out int number) || number < 1 || number > allGenres.Count)
            {
                error = $"Invalid input. Enter a number between 1 and {allGenres.Count}.";
                return null;
            }

            error = null;
            return allGenres[number - 1];
        }
    }
}