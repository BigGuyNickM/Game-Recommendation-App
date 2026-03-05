using Game_Recommendation.Cli.Config;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Models;
using Game_Recommendation.Database.Repositories;
using System;

namespace Game_Recommendation.Cli.Managers.Auth
{
    public class AuthManager
    {
        private readonly UserRepository _userRepo;

        public AuthManager(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // NOTE:
        // We used Bcrypt for password hashing/encryption to keep things simple and secure

        #region Public

        public User Login()
        {
            string username = _GetValidatedInput(
                header: "LOGIN",
                question: "Please enter your username:",
                validate: input =>
                {
                    if (string.IsNullOrWhiteSpace(input)) return "Username cannot be empty.";
                    if (!_userRepo.UsernameExists(input)) return $"Username '{input}' not found. Please try again.";
                    return null;
                }
            );
            if (username == null) return null;

            string storedHash = _userRepo.GetPasswordHash(username);
            string password = _GetValidatedInput(
                header: "LOGIN",
                question: $"Hello, {username}!\n\nPlease enter your password:",
                validate: input =>
                {
                    if (string.IsNullOrWhiteSpace(input)) return "Password cannot be empty.";
                    if (!BCrypt.Net.BCrypt.Verify(input, storedHash)) return "Incorrect password. Please try again.";
                    return null;
                },
                masked: true
            );
            if (password == null) return null;

            var user = _userRepo.GetUserByUsername(username);
            ConsoleHelper.PrintHeader("LOGIN");
            ConsoleHelper.PrintColored($"Welcome back, {user.Username}!", AppConfig.Success);
            return user;
        }

        public User Signup()
        {
            string username = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please choose a username:",
                validate: input =>
                {
                    if (ValidationHelper.ValidateUsername(input) is string err) return err;
                    if (_userRepo.UsernameExists(input)) return $"Username '{input}' is already taken.";
                    return null;
                }
            );
            if (username == null) return null;

            string email = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please enter your email:",
                validate: input => ValidationHelper.ValidateEmail(input)
            );
            if (email == null) return null;

            string password = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please choose a password:",
                validate: input => ValidationHelper.ValidatePassword(input),
                masked: true,
                showStrength: true
            );
            if (password == null) return null;

            string confirm = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please re-enter your password:",
                validate: input => input != password ? "Passwords do not match. Please try again." : null,
                masked: true
            );
            if (confirm == null) return null;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var newUser = _userRepo.CreateUser(username, email, passwordHash);
            newUser.IsNewUser = true;

            ConsoleHelper.PrintHeader("SIGN UP");
            ConsoleHelper.PrintColored($"Account created! Welcome, {newUser.Username}.", AppConfig.Success);
            InputHelper.WaitForKey("\nLet's set up your genre preferences.\nPress any key to continue...");

            return newUser;
        }

        #endregion

        #region Helpers

        // Loops until input passes validation or user hits 0 to go back
        // Returns null if user backs out, otherwise returns the valid input
        private string _GetValidatedInput(
            string header,
            string question,
            Func<string, string> validate,
            bool masked = false,
            bool showStrength = false)
        {
            string error = null;
            while (true)
            {
                ConsoleHelper.PrintHeader(header);
                ConsoleHelper.PrintOptions("0", "Back");
                Console.WriteLine(question + "\n");

                if (error != null) ConsoleHelper.PrintError(error + "\n");

                string input = masked
                    ? InputHelper.GetMaskedInput("", showStrength)
                    : InputHelper.GetInput();

                if (input == "0") return null;

                error = validate(input);
                if (error == null) return input;
            }
        }

        #endregion
    }
}