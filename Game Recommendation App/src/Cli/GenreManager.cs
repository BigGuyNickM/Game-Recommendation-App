using System;
using System.Collections.Generic;
using System.Linq;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Repositories;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli
{
    public class GenreManager
    {
        private GenreRepository genreRepo;

        public GenreManager()
        {
            genreRepo = new GenreRepository();
        }

        // For signup: Select preferences (must pick at least 3)
        public void SelectPreferences(int userId)
        {
            List<Genre> allGenres = genreRepo.GetAllGenres();
            List<int> selectedGenreIds = new List<int>();

            bool selecting = true;

            while (selecting)
            {
                _DisplaySelectionScreen(allGenres, selectedGenreIds, "SELECT YOUR FAVORITE GENRES", "Pick at least 3 genres you enjoy");

                string input = InputHelper.GetInput("\nEnter genre number to toggle selection (or 'done' to finish): ").Trim().ToLower();

                if (input == "done")
                {
                    if (selectedGenreIds.Count < 3)
                    {
                        Console.WriteLine("Please select at least 3 genres before finishing."); // Error if below minimum
                        System.Threading.Thread.Sleep(1500);
                    }
                    else
                    {
                        selecting = false;
                    }
                }
                else if (int.TryParse(input, out int genreNumber))
                {
                    _ToggleGenreInList(allGenres, selectedGenreIds, genreNumber); // Display successful change
                }
                else
                {
                    Console.WriteLine("Invalid input. Enter a number or 'done'."); // Error if invalid input
                    System.Threading.Thread.Sleep(1000);
                }
            }

            genreRepo.SaveUserPreferences(userId, selectedGenreIds);
            Console.WriteLine("\nPreferences saved!");
        }

        // Modify existing preferences (add/remove)
        public void ManagePreferences(int userId)
        {
            bool managing = true;

            while (managing)
            {
                List<Genre> allGenres = genreRepo.GetAllGenres();
                List<Genre> userGenres = genreRepo.GetUserPreferredGenres(userId);
                List<int> userGenreIds = userGenres.Select(g => g.Id).ToList();

                _DisplaySelectionScreen(allGenres, userGenreIds, "MANAGE PREFERRED GENRES", $"You have {userGenreIds.Count} genre(s) selected (minimum 3 required)");

                Console.WriteLine("\nOptions:");
                Console.WriteLine("Enter a number to add/remove that genre");
                Console.WriteLine("Type 'done' to finish");

                string input = InputHelper.GetInput("\nYour choice: ").ToLower();

                if (input == "done")
                {
                    managing = false;
                    continue;
                }
                else if (int.TryParse(input, out int genreNumber))
                {
                    _ToggleGenreInDatabase(allGenres, userGenreIds, genreNumber, userId);
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                    System.Threading.Thread.Sleep(1000);
                }
            }

            Console.WriteLine("\nPreferences updated!");
        }

        // Helper method: Display genres with checkboxes
        private void _DisplaySelectionScreen(List<Genre> allGenres, List<int> selectedGenreIds, string title, string subtitle)
        {
            Console.Clear();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine(subtitle + "\n");

            for (int i = 0; i < allGenres.Count; i++)
            {
                Genre genre = allGenres[i];
                int displayNumber = i + 1;

                bool isSelected = selectedGenreIds.Contains(genre.Id);
                string checkbox = isSelected ? "[X]" : "[ ]";

                Console.WriteLine($"{displayNumber}. {checkbox} {genre.GenreName}");
            }

            Console.WriteLine($"\nSelected: {selectedGenreIds.Count}");
        }



        // TODO: Extract duplicated code into util method for genre selection display.

        // Helper method: Toggle genre in memory list (for signup)
        private void _ToggleGenreInList(List<Genre> allGenres, List<int> selectedGenreIds, int genreNumber)
        {
            int index = genreNumber - 1;

            if (index < 0 || index >= allGenres.Count)
            {
                Console.WriteLine("Invalid genre number.");
                System.Threading.Thread.Sleep(1000);
                return;
            }

            Genre selectedGenre = allGenres[index];

            if (selectedGenreIds.Contains(selectedGenre.Id)) // Display added genre
            {
                selectedGenreIds.Remove(selectedGenre.Id);
                Console.WriteLine($"Removed {selectedGenre.GenreName}");
            }
            else // Display removed genre
            {
                selectedGenreIds.Add(selectedGenre.Id);
                Console.WriteLine($"Added {selectedGenre.GenreName}");
            }

            System.Threading.Thread.Sleep(800);
        }

        // Helper method: Toggle genre in database (for manage)
        private void _ToggleGenreInDatabase(List<Genre> allGenres, List<int> userGenreIds, int genreNumber, int userId)
        {
            int index = genreNumber - 1;

            if (index < 0 || index >= allGenres.Count)
            {
                Console.WriteLine("Invalid genre number.");
                System.Threading.Thread.Sleep(1000);
                return;
            }

            Genre selectedGenre = allGenres[index];

            if (userGenreIds.Contains(selectedGenre.Id))
            {
                // Check if removing would go below the minimum required
                if (userGenreIds.Count <= 3)
                {
                    Console.WriteLine("Cannot remove - you must have at least 3 genres.");
                    System.Threading.Thread.Sleep(1500);
                    return;
                }

                genreRepo.RemoveUserPreference(userId, selectedGenre.Id);
                Console.WriteLine($"Removed {selectedGenre.GenreName}"); // Display removed genre
            }
            else
            {
                genreRepo.AddUserPreference(userId, selectedGenre.Id);
                Console.WriteLine($"Added {selectedGenre.GenreName}"); // Display added genre
            }

            System.Threading.Thread.Sleep(800);
        }
    }
}