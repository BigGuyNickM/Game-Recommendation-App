using System.Text.RegularExpressions;

namespace Game_Recommendation.Cli.Utils
{
    internal static class ValidationHelper
    {
        public static bool IsValidUsername(string username, out string error)
        {
            if (username.Length < 3 || username.Length > 30)
            {
                error = "Username must be between 3 and 30 characters.";
                return false;
            }
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
            {
                error = "Username can only contain letters and numbers.";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsValidEmail(string email, out string error)
        {
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$"))
            {
                error = "Please enter a valid email address (e.g. user@example.com).";
                return false;
            }
            error = null;
            return true;
        }

        public static bool IsValidPassword(string password, out string error)
        {
            if (password.Length < 6)
            {
                error = "Password must be at least 6 characters.";
                return false;
            }
            error = null;
            return true;
        }

        public static (string label, int score) GetPasswordStrength(string password)
        {
            int score = 0;
            if (password.Length >= 6) score++;
            if (password.Length >= 10) score++;
            if (Regex.IsMatch(password, @"[A-Z]") && Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++;
            if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;

            string label = score <= 1 ? "Weak" : score <= 3 ? "Medium" : "Strong";
            return (label, score);
        }
    }
}