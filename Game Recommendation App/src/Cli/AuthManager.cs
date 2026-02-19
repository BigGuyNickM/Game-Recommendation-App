using System;
using Game_Recommendation.Cli.Utils;
using Game_Recommendation.Repositories;
using Game_Recommendation.Models;

namespace Game_Recommendation.Cli
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
            Console.Clear();
            Console.WriteLine("=== LOGIN / SIGNUP ===\n");

            string username = InputHelper.GetInput("Enter your username: ");

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty.");
                return null;
            }

            if (userRepo.UsernameExists(username))
            {
                User user = userRepo.GetUserByUsername(username);
                Console.WriteLine($"Welcome back, {user.Username}!");
                return user;
            }
            else // If username doesn't exist, prompt to sign up
            {
                Console.WriteLine($"\nUsername '{username}' not found.");
                if (InputHelper.Confirm("Would you like to sign up? (y/n): "))
                    return Signup(username);
                return null;
            }
        }

        private User Signup(string username)
        {
            Console.WriteLine("\n=== SIGN UP ===\n");

            string email = InputHelper.GetInput("Enter your email: "); // We should change this to asking if they have an email

            // Checks if email is blank, should add more error handling such as needing "@email.com"
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email cannot be empty. Signup cancelled");
                return null;
            }

            User newUser = userRepo.CreateUser(username, email);

            Console.WriteLine($"\nAccount created successfully!\nWelcome, {newUser.Username}.\n");
    
            ConsoleHelper.WaitForKey("\nLet's set up your game preferences.\nPress any key to continue...");

            GenreManager genreManager = new GenreManager();
            genreManager.SelectPreferences(newUser.Id);

            return newUser;
        }
    }
}
