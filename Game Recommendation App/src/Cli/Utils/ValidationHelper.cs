using Game_Recommendation.Cli.Config;
using System.Text.RegularExpressions;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ValidationHelper
    {
        public static string? ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Username cannot be empty.";
            if (username.Length < AppConfig.UsernameMinLength || username.Length > AppConfig.UsernameMaxLength)
                return $"Username must be between {AppConfig.UsernameMinLength} and {AppConfig.UsernameMaxLength} characters.";
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
                return "Username can only contain letters and numbers.";
            return null;
        }

        public static string? ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email cannot be empty.";
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$"))
                return "Please enter a valid email address (e.g. user@example.com).";
            return null;
        }

        public static string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password cannot be empty.";
            if (password.Length < AppConfig.PasswordMinLength)
                return $"Password must be at least {AppConfig.PasswordMinLength} characters.";
            if (GetPasswordStrength(password).score <= AppConfig.PasswordStrengthMedium)
                return "Password is too weak. Try adding uppercase letters, numbers, or symbols.";
            return null;
        }

        public static (string label, int score) GetPasswordStrength(string password)
        {
            int score = 0;
            if (password.Length >= 6) score++;
            if (password.Length >= 10) score++;
            if (Regex.IsMatch(password, @"[A-Z]") && Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;
            string label = score switch
            {
                <= AppConfig.PasswordStrengthWeak => "Weak",
                <= AppConfig.PasswordStrengthMedium => "Medium",
                _ => "Strong"
            };
            return (label, score);
        }
    }
}