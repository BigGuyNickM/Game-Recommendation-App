using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Game_Recommendation.Services
{
    public class RawgApiService
    {
        private readonly string apiKey;
        private readonly string baseUrl = "https://api.rawg.io/api";
        private readonly HttpClient httpClient;

        public RawgApiService()
        {
            apiKey = ConfigurationManager.AppSettings["RawgApiKey"]?.Trim();
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException(
                    "RAWG API key is missing. Add a 'RawgApiKey' entry in App.config under <appSettings>. Get a free key at https://rawg.io/apidocs");
            httpClient = new HttpClient();
        }

        // Fetch games with pagination
        public async Task<List<RawgGame>> FetchGamesAsync(int totalGamesToFetch = 20000)
        {
            List<RawgGame> allGames = new List<RawgGame>();
            int pageSize = 40; // RAWG allows up to 40 games per page
            int currentPage = 1;
            int gamesRetrieved = 0;

            Console.WriteLine($"Starting to fetch {totalGamesToFetch} games from RAWG API...");
            Console.WriteLine("This may take 5-10 minutes. Please be patient.\n");

            while (gamesRetrieved < totalGamesToFetch)
            {
                try
                {
                    // Build the API URL with filters
                    string url = $"{baseUrl}/games?key={apiKey}&page={currentPage}&page_size={pageSize}&ordering=-metacritic";

                    // Make the HTTP request
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        string message = GetHttpErrorMessage(response.StatusCode, errorBody, currentPage);
                        Console.WriteLine("Error: " + message);
                        break;
                    }

                    // Parse the JSON response
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(jsonResponse);
                    JArray results = (JArray)data["results"];

                    if (results == null || results.Count == 0)
                    {
                        Console.WriteLine("No more games available from API.");
                        break;
                    }

                    // Process each game in the response
                    foreach (JToken gameJson in results)
                    {
                        RawgGame game = ParseGame(gameJson);
                        if (game != null)
                        {
                            allGames.Add(game);
                            gamesRetrieved++;

                            if (gamesRetrieved >= totalGamesToFetch)
                                break;
                        }
                    }

                    // Progress update
                    Console.WriteLine($"Fetched {gamesRetrieved}/{totalGamesToFetch} games (Page {currentPage})");

                    currentPage++;

                    // Small delay to avoid hitting rate limits
                    await Task.Delay(100);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error (page {currentPage}): {ex.Message}");
                    Console.WriteLine("Check your internet connection and try again.");
                    break;
                }
                catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested == false)
                {
                    Console.WriteLine($"Request timed out (page {currentPage}). The API may be slow or unreachable.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error fetching page {currentPage}: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine($"\nTotal games fetched: {allGames.Count}");
            return allGames;
        }

        private static string GetHttpErrorMessage(HttpStatusCode statusCode, string responseBody, int page)
        {
            switch ((int)statusCode)
            {
                case 401:
                    return "Invalid or missing RAWG API key. Check the 'RawgApiKey' value in App.config.";
                case 403:
                    return "Access denied by RAWG API. Your key may be restricted or invalid.";
                case 429:
                    return $"Rate limit exceeded (page {page}). Wait a few minutes before fetching again.";
                case 500:
                case 502:
                case 503:
                    return $"RAWG API server error (page {page}). Try again later.";
                default:
                    return $"API returned {(int)statusCode} (page {page}). {responseBody?.Substring(0, Math.Min(100, responseBody?.Length ?? 0))}";
            }
        }

        // Parse individual game from JSON
        private RawgGame ParseGame(JToken gameJson)
        {
            try
            {
                RawgGame game = new RawgGame
                {
                    Title = gameJson["name"]?.ToString(),
                    ReleaseDate = gameJson["released"]?.ToString(),
                    Rating = gameJson["rating"]?.ToObject<decimal?>() ?? 0,
                    RatingsCount = gameJson["ratings_count"]?.ToObject<int?>() ?? 0,
                    Metacritic = gameJson["metacritic"]?.ToObject<int?>(),
                    Description = gameJson["description_raw"]?.ToString() ?? "",
                    BackgroundImage = gameJson["background_image"]?.ToString(),
                    Genres = new List<string>()
                };

                // Parse genres
                JArray genres = (JArray)gameJson["genres"];
                if (genres != null)
                {
                    foreach (JToken genre in genres)
                    {
                        string genreName = genre["name"]?.ToString();
                        if (!string.IsNullOrEmpty(genreName))
                        {
                            game.Genres.Add(genreName);
                        }
                    }
                }

                return game;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing game: {ex.Message}");
                return null;
            }
        }
    }

    // Simple class to hold game data from API
    public class RawgGame
    {
        public string Title { get; set; }
        public string ReleaseDate { get; set; }
        public decimal Rating { get; set; }
        public int RatingsCount { get; set; }
        public int? Metacritic { get; set; }
        public string Description { get; set; }
        public string BackgroundImage { get; set; }
        public List<string> Genres { get; set; }
    }
}