using Game_Recommendation.Cli.Config;
using Game_Recommendation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ConsoleHelper
    {
        #region Output

        public static void PrintColored(string text, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
            if (newLine) Console.WriteLine();
        }

        // Overload for multiple colored segments
        public static void PrintColored(params (string text, ConsoleColor color)[] segments)
        {
            foreach (var (text, color) in segments)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void PrintError(string message) => PrintColored(message, AppConfig.Error);

        public static void PrintHeader(string title)
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J"); // clear scrollback buffer
            int width = Math.Max(AppConfig.HeaderMinWidth, title.Length + AppConfig.HeaderPadding + 2);
            int innerWidth = width - 2;
            string centeredTitle = title.PadLeft((innerWidth + title.Length) / 2).PadRight(innerWidth);
            PrintColored(
                $"╔{new string('═', innerWidth)}╗\n" +
                $"║{centeredTitle}║\n" +
                $"╚{new string('═', innerWidth)}╝\n",
                AppConfig.Header
            );
        }

        public static void PrintPasswordStrength(string label, int score)
        {
            ConsoleColor color = score switch
            {
                <= 1 => AppConfig.Error,
                <= 3 => AppConfig.Input,
                _ => AppConfig.Success,
            };
            PrintColored(("Password strength: ", AppConfig.Highlight), (label, color));
        }

        #endregion

        #region Options

        // Prints options vertically: [A] Option 1    [B] Option 2
        public static void PrintOptions(params (string key, string label)[] options)
        {
            foreach (var (key, label) in options)
                PrintColored(($"[{key}]", AppConfig.Input), ($" {label}\n", AppConfig.Default));
        }

        // Overload for a single option
        public static void PrintOptions(string key, string label)
        {
            PrintColored(($"[{key}]", AppConfig.Input), ($" {label}\n", AppConfig.Default));
        }

        // Overload for printing options as a grid
        public static void PrintOptions(Action[] items, int columns = AppConfig.DefaultGridColumns)
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i]();
                if ((i + 1) % columns == 0 || i == items.Length - 1)
                    Console.WriteLine();
                else
                    Console.Write("   ");
            }
        }

        #endregion

        #region Game Display

        public static string BuildGameCard(Game game, int numberWidth)
        {
            int inner = AppConfig.GameCardWidth - 1;
            string border = new string('─', AppConfig.GameCardWidth);
            string title = _Truncate(game.Title, inner);
            string genres = _FormatGenres(game.Genres, inner);
            string rating = game.AvgRating.HasValue ? $"★ {game.AvgRating.Value:0.00}" : "★ N/A";
            string pad = "".PadRight(numberWidth + 3);

            return
                $"┌{border}┐\n" +
                $"{pad}│ {title.PadRight(inner)}│\n" +
                $"{pad}│ {genres.PadRight(inner)}│\n" +
                $"{pad}│ {rating.PadRight(inner)}│\n" +
                $"{pad}└{border}┘";
        }

        public static void PrintGameGrid(List<Game> games, int page, int totalPages, int columns = AppConfig.DefaultGridColumns)
        {
            int start = page * AppConfig.GamePageSize;
            var pageGames = games.Skip(start).Take(AppConfig.GamePageSize).ToList();
            int numberWidth = (start + pageGames.Count).ToString().Length;
            int cardTotalWidth = AppConfig.GameCardWidth + numberWidth + 6;

            var rows = pageGames
                .Select((game, i) => new { game, i })
                .GroupBy(x => x.i / columns)
                .Select(g => g.Select(x => x.game).ToList())
                .ToList();

            int runningCount = 0;
            foreach (var row in rows)
            {
                var lines = row.Select(game => BuildGameCard(game, numberWidth).Split('\n')).ToArray();
                var numbers = row.Select((_, i) => start + runningCount + i + 1).ToArray();
                _PrintCardLines(lines, numbers, numberWidth, cardTotalWidth);
                runningCount += row.Count;
            }

            PrintColored($"\nPage {page + 1} of {totalPages}\n", AppConfig.Muted);
            if (page > 0) PrintOptions("A", "Previous Page");
            if (page < totalPages - 1) PrintOptions("D", "Next Page");
            PrintOptions("0", "Back\n");
        }

        #endregion

        #region Helpers

        // Prints all lines of a card row, with numbered labels on the first line
        private static void _PrintCardLines(string[][] lines, int[] numbers, int numberWidth, int cardTotalWidth)
        {
            for (int line = 0; line < lines[0].Length; line++)
            {
                for (int col = 0; col < lines.Length; col++)
                {
                    if (line == 0)
                    {
                        string num = $"[{numbers[col]}]".PadRight(numberWidth + 2);
                        PrintColored(num, AppConfig.Input, newLine: false);
                        Console.Write((" " + lines[col][line]).PadRight(cardTotalWidth - num.Length));
                    }
                    else
                    {
                        Console.Write(lines[col][line].PadRight(cardTotalWidth));
                    }
                    if (col < lines.Length - 1) Console.Write("   ");
                }
                Console.WriteLine();
            }
        }

        private static string _Truncate(string text, int maxLength) =>
            text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";

        private static string _FormatGenres(List<string> genres, int maxWidth)
        {
            if (genres.Count == 0) return "No genres";

            var shown = new List<string>();
            foreach (string genre in genres)
            {
                if (shown.Count >= AppConfig.GameCardGenreLimit) break;
                shown.Add(genre);
                int remaining = genres.Count - shown.Count;
                string candidate = string.Join(", ", shown) + (remaining > 0 ? $" +{remaining} more" : "");
                if (candidate.Length > maxWidth) { shown.RemoveAt(shown.Count - 1); break; }
            }

            int left = genres.Count - shown.Count;
            return string.Join(", ", shown) + (left > 0 ? $" +{left} more" : "");
        }

        #endregion
    }
}