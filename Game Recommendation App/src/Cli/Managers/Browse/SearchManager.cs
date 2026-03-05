using System;
using System.Collections.Generic;
using System.Linq;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Database.Repositories;
using Game_Recommendation.Services;

namespace Game_Recommendation.Cli.Managers.Browse
{
    public class SearchManager : BaseMenu
    {
        private readonly SearchService _searchService;
        private readonly GenreRepository _genreRepo;
        private readonly GameDisplayManager _gameDisplayManager;

        private List<Genre> _allGenres;
        private List<int> _selectedGenreIds;
        private string _error;

        public SearchManager(GameRepository gameRepo, GenreRepository genreRepo, GameDisplayManager gameDisplayManager)
        {
            _searchService = new SearchService(gameRepo);
            _genreRepo = genreRepo;
            _gameDisplayManager = gameDisplayManager;
            _allGenres = new List<Genre>();
            _selectedGenreIds = new List<int>();
            _error = null;
        }

        #region Menu

        protected override void _ShowMenu()
        {
            ConsoleHelper.PrintHeader("SEARCH GAMES");
            ConsoleHelper.PrintOptions("1", "Filter by Genre");
            if (_selectedGenreIds.Count > 0) ConsoleHelper.PrintOptions("2", "Clear Filters");
            ConsoleHelper.PrintOptions("0", "Back\n");
            _PrintActiveFilters();
            ConsoleHelper.PrintColored("Or type a keyword to search:\n", AppConfig.Muted);

            if (_error == null) return;
            ConsoleHelper.PrintError(_error + "\n");
            _error = null;
        }

        protected override void _HandleChoice(string choice)
        {
            switch (choice)
            {
                case "0": _Exit(); break;
                case "1": _FilterByGenre(); break;
                case "2": if (_selectedGenreIds.Count > 0) _selectedGenreIds.Clear(); break;
                default:
                    if (string.IsNullOrWhiteSpace(choice)) _error = "Please enter a keyword or select an option.";
                    else _RunSearch(choice.ToLower().Trim());
                    break;
            }
        }

        #endregion

        #region Helpers

        private void _FilterByGenre()
        {
            // Lazy load the genres; Only fetch once and reuse them
            if (_allGenres.Count == 0) _allGenres = _genreRepo.GetAllGenres();

            string error = null;
            while (true)
            {
                ConsoleHelper.PrintHeader("FILTER BY GENRE");
                ConsoleHelper.PrintColored("Toggle genres to filter by:\n", AppConfig.Default);
                _PrintGenreGrid();
                ConsoleHelper.PrintColored($"\nSelected: {_selectedGenreIds.Count}\n", AppConfig.Default);
                ConsoleHelper.PrintOptions("0", "Done");

                if (error != null) { ConsoleHelper.PrintError("\n" + error); error = null; }

                string input = InputHelper.GetInput();
                if (input == "0") return;
                if (!int.TryParse(input, out int number) || number < 1 || number > _allGenres.Count)
                {
                    error = $"Invalid input. Enter a number between 1 and {_allGenres.Count}.";
                    continue;
                }

                var selected = _allGenres[number - 1];
                if (_selectedGenreIds.Contains(selected.Id)) _selectedGenreIds.Remove(selected.Id);
                else _selectedGenreIds.Add(selected.Id);
            }
        }

        private void _RunSearch(string keyword)
        {
            var results = _searchService.Search(_selectedGenreIds, new List<string> { keyword });
            _gameDisplayManager.ShowResults(results);
        }

        // Print the selected genres as active filters, e.g. "Genres: Action, RPG"
        private void _PrintActiveFilters()
        {
            if (_selectedGenreIds.Count == 0 || _allGenres.Count == 0) return;

            var selectedNames = _allGenres
                .Where(g => _selectedGenreIds.Contains(g.Id))
                .Select(g => g.GenreName)
                .ToList();

            ConsoleHelper.PrintColored("Genres:  ", AppConfig.Muted, newLine: false);
            ConsoleHelper.PrintColored(string.Join(", ", selectedNames) + "\n", AppConfig.Highlight);
        }

        // Display genres in a grid with checkboxes, e.g. "[1] (X) Action" (could possibly be refactored to a reusable component since used in multiple places)
        private void _PrintGenreGrid()
        {
            int maxNameLength = _allGenres.Max(g => g.GenreName.Length);
            int numberWidth = _allGenres.Count.ToString().Length;

            var items = _allGenres.Select((genre, i) =>
            {
                bool isSelected = _selectedGenreIds.Contains(genre.Id);
                string checkbox = isSelected ? "(X) " : "( ) ";
                ConsoleColor checkColor = isSelected ? AppConfig.Success : AppConfig.Default;
                ConsoleColor nameColor = isSelected ? AppConfig.Highlight : AppConfig.Default;

                return (Action)(() =>
                {
                    ConsoleHelper.PrintColored($"[{i + 1}]".PadRight(numberWidth + 3), AppConfig.Input, newLine: false);
                    ConsoleHelper.PrintColored(checkbox, checkColor, newLine: false);
                    ConsoleHelper.PrintColored(genre.GenreName.PadRight(maxNameLength), nameColor, newLine: false);
                });
            }).ToArray();

            ConsoleHelper.PrintOptions(items, AppConfig.DefaultGridColumns);
        }

        #endregion
    }
}