using Game_Recommendation.Cli.Config;
using Game_Recommendation.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ConsoleHelper
    {
        public static void PrintHeader(string title)
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            int width = Math.Max(AppConfig.HeaderMinWidth, title.Length + (AppConfig.HeaderPadding + 2));
            int innerWidth = width - 2;
            string centeredTitle = title.PadLeft((innerWidth + title.Length) / 2).PadRight(innerWidth);
            string top = $"╔{new string('═', innerWidth)}╗";
            string middle = $"║{centeredTitle}║";
            string bottom = $"╚{new string('═', innerWidth)}╝";
            PrintColored(top + "\n" + middle + "\n" + bottom + "\n", AppConfig.Header);
        }

        // Prints a single colored segment
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

        public static void PrintError(string message)
        {
            PrintColored(message, AppConfig.Error);
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

        // Prints options vertically: [1] Option 1    [2] Option 2    [0] Exit
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

        // --- Game Display ---
        
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
            List<Game> pageGames = games.Skip(start).Take(AppConfig.GamePageSize).ToList();
            int numberWidth = (start + pageGames.Count).ToString().Length;
            int cardTotalWidth = AppConfig.GameCardWidth + numberWidth + 6;

            List<List<Game>> rows = pageGames
                .Select((game, i) => new { game, i })
                .GroupBy(x => x.i / columns)
                .Select(g => g.Select(x => x.game).ToList())
                .ToList();

            foreach (List<Game> row in rows)
            {
                string[][] lines = row
                    .Select((game, i) => BuildGameCard(game, numberWidth).Split('\n'))
                    .ToArray();

                int[] numbers = row
                    .Select((_, i) => start + rows.Take(rows.IndexOf(row)).Sum(r => r.Count) + i + 1)
                    .ToArray();

                _PrintCardRow(lines, numbers, numberWidth, cardTotalWidth);
                Console.WriteLine();
            }

            PrintColored($"Page {page + 1} of {totalPages}\n", AppConfig.Muted);
            if (page > 0) PrintOptions("A", "Previous Page");
            if (page < totalPages - 1) PrintOptions("D", "Next Page");
            PrintOptions("0", "Back\n");
        }

        private static void _PrintCardRow(string[][] lines, int[] numbers, int numberWidth, int cardTotalWidth)
        {
            for (int line = 0; line < lines[0].Length; line++)
                _PrintCardLine(lines, numbers, line, numberWidth, cardTotalWidth);
        }

        private static void _PrintCardLine(string[][] lines, int[] numbers, int line, int numberWidth, int cardTotalWidth)
        {
            for (int col = 0; col < lines.Length; col++)
            {
                _PrintCardCell(lines[col][line], numbers[col], line, numberWidth, cardTotalWidth);
                if (col < lines.Length - 1) Console.Write("   ");
            }
            Console.WriteLine();
        }

        private static void _PrintCardCell(string content, int number, int line, int numberWidth, int cardTotalWidth)
        {
            if (line != 0)
            {
                Console.Write(content.PadRight(cardTotalWidth));
                return;
            }

            string num = $"[{number}]".PadRight(numberWidth + 2);
            PrintColored(num, AppConfig.Input, newLine: false);
            Console.Write((" " + content).PadRight(cardTotalWidth - num.Length));
        }

        private static string _Truncate(string text, int maxLength)
        {
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private static string _FormatGenres(List<string> genres, int maxWidth)
        {
            if (genres.Count == 0) return "No genres";

            List<string> shown = new List<string>();
            foreach (string genre in genres)
            {
                if (shown.Count >= AppConfig.GameCardGenreLimit) break;
                int extra = genres.Count - shown.Count - 1;
                string suffix = extra > 0 ? $" +{extra} more" : "";
                string candidate = string.Join(", ", shown.Append(genre)) + suffix;
                if (candidate.Length > maxWidth) break;
                shown.Add(genre);
            }

            int remaining = genres.Count - shown.Count;
            return string.Join(", ", shown) + (remaining > 0 ? $" +{remaining} more" : "");
        }
    }
}