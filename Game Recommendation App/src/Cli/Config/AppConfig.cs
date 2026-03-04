using System;

namespace Game_Recommendation.Cli.Config
{
    internal static class AppConfig
    {
        #region Colors
        public static readonly ConsoleColor Header = ConsoleColor.Cyan;
        public static readonly ConsoleColor Input = ConsoleColor.Yellow;
        public static readonly ConsoleColor Highlight = ConsoleColor.White;
        public static readonly ConsoleColor Default = ConsoleColor.Gray;
        public static readonly ConsoleColor Success = ConsoleColor.Green;
        public static readonly ConsoleColor Error = ConsoleColor.Red;
        public static readonly ConsoleColor Muted = ConsoleColor.DarkGray;
        #endregion

        #region Layout
        public const int HeaderMinWidth = 15;
        public const int HeaderPadding = 5;
        public const int DefaultGridColumns = 3;
        public const int DefaultPageSize = 10;
        public const int GamePageSize = 9;
        #endregion

        #region Game Cards
        public const int GameCardWidth = 25;
        public const int GameCardGenreLimit = 2;
        #endregion

        #region Profile
        public const int ProfilePreviewCount = 3;
        #endregion

        #region Genre Selection
        public const int MinGenreSelections = 3;
        #endregion

        #region Search
        public const int FuzzyMatchTolerance = 2;
        public const int FuzzyMatchMinLength = 4;
        #endregion

        #region Validation
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 30;
        public const int PasswordMinLength = 6;
        public const int PasswordStrengthWeak = 1;
        public const int PasswordStrengthMedium = 3;
        #endregion

        #region Caching
        public const int GameCacheMinutes = 30;
        #endregion

        #region Timing
        public const int TransitionDelay = 500;
        #endregion
    }
}