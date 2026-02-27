using System;
using BCrypt.Net;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Cli.Config;
using Game_Recommendation.Repositories;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli.Managers
{
    public class AuthManager
    {
        private UserRepository userRepo;

        public AuthManager()
        {
            userRepo = new UserRepository();
        }

        public User Login()
        {
            string username = _GetValidatedInput(
                header: "LOGIN",
                question: "Please enter your username:",
                validate: input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return "Username cannot be empty.";
                    if (!userRepo.UsernameExists(input))
                        return $"Username '{input}' not found. Please try again.";
                    return null;
                }
            );
            if (username == null) return null;

            string storedHash = userRepo.GetPasswordHash(username);

            _GetValidatedInput(
                header: "LOGIN",
                question: $"Hello, {username}!\n\nPlease enter your password:",
                validate: input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return "Password cannot be empty.";
                    if (!BCrypt.Net.BCrypt.Verify(input, storedHash))
                        return "Incorrect password. Please try again.";
                    return null;
                },
                masked: true
            );

            User user = userRepo.GetUserByUsername(username);

            ConsoleHelper.PrintHeader("LOGIN");
            ConsoleHelper.PrintColored($"Welcome back, {user.Username}!", ColorScheme.Success);

            return user;
        }

        public User Signup()
        {
            string username = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please choose a username:",
                validate: input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return "Username cannot be empty.";
                    if (!ValidationHelper.IsValidUsername(input, out string err))
                        return err;
                    if (userRepo.UsernameExists(input))
                        return $"Username '{input}' is already taken.";
                    return null;
                }
            );
            if (username == null) return null;

            string email = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please enter your email:",
                validate: input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return "Email cannot be empty.";
                    if (!ValidationHelper.IsValidEmail(input, out string err))
                        return err;
                    return null;
                }
            );
            if (email == null) return null;

            string password = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please choose a password:",
                validate: input =>
                {
                    if (!ValidationHelper.IsValidPassword(input, out string err))
                        return err;
                    return null;
                },
                masked: true,
                showStrength: true
            );
            if (password == null) return null;

            string confirm = _GetValidatedInput(
                header: "SIGN UP",
                question: "Please re-enter your password:",
                validate: input =>
                {
                    if (input != password)
                        return "Passwords do not match. Please try again.";
                    return null;
                },
                masked: true
            );
            if (confirm == null) return null;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            User newUser = userRepo.CreateUser(username, email, passwordHash);
            newUser.IsNewUser = true;

            ConsoleHelper.PrintHeader("SIGN UP");
            ConsoleHelper.PrintColored($"Account created! Welcome, {newUser.Username}.", ColorScheme.Success);
            InputHelper.WaitForKey("\nLet's set up your genre preferences.\nPress any key to continue...");
            
            return newUser;
        }

        private string _GetValidatedInput(
            string header,
            string question,
            Func<string, string> validate,
            bool masked = false,
            bool showStrength = false
            )
        {
            string error = null;
            while (true)
            {
                ConsoleHelper.PrintHeader(header);

                ConsoleHelper.PrintLine(("[0] ", ColorScheme.Input), ("Back\n", ColorScheme.Highlight));
                Console.WriteLine(question + "\n");

                if (error != null)
                    ConsoleHelper.PrintError(error + "\n");

                

                string input = masked
                    ? InputHelper.GetMaskedInput("", showStrength)
                    : InputHelper.GetInput();

                if (input == "0") return null;

                error = validate(input);
                if (error == null)
                    return input;
            }
        }
    }
}