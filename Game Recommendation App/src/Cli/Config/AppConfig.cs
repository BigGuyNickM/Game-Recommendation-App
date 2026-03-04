using System;

namespace Game_Recommendation.Cli.Config
{
    internal static class AppConfig
    {
        // Colors
        public static readonly ConsoleColor Header = ConsoleColor.Cyan;
        public static readonly ConsoleColor Input = ConsoleColor.Yellow;
        public static readonly ConsoleColor Highlight = ConsoleColor.White;
        public static readonly ConsoleColor Default = ConsoleColor.Gray;
        public static readonly ConsoleColor Success = ConsoleColor.Green;
        public static readonly ConsoleColor Error = ConsoleColor.Red;
        public static readonly ConsoleColor Muted = ConsoleColor.DarkGray;

        // Header layout
        public const int HeaderMinWidth = 15;
        public const int HeaderPadding = 5;

        // Grid layout
        public const int DefaultGridColumns = 3;

        // Genre selection
        public const int MinGenreSelections = 3;

        // Pagination
        public const int DefaultPageSize = 10;

        // Username validation
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 30;

        // Password validation
        public const int PasswordMinLength = 6;
        public const int PasswordStrengthWeak = 1;
        public const int PasswordStrengthMedium = 3;

        // Timing
        public const int TransitionDelay = 500;

        // Search
        public const int FuzzyMatchTolerance = 2;

        // Game cards
        public const int GameCardWidth = 25;
        public const int GameCardGenreLimit = 2;
    }
}