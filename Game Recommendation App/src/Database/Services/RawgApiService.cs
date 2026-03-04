using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Game_Recommendation.Database.Services
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
            List<RawgGame> allGames = new();
            int pageSize = 40;
            int currentPage = 1;
            int gamesRetrieved = 0;

            Console.WriteLine($"Starting to fetch {totalGamesToFetch} games from RAWG API...");
            Console.WriteLine("This may take 5-10 minutes. Please be patient.\n");

            while (gamesRetrieved < totalGamesToFetch)
            {
                try
                {
                    string url = $"{baseUrl}/games?key={apiKey}&page={currentPage}&page_size={pageSize}&ordering=-metacritic";
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Error: " + GetHttpErrorMessage(response.StatusCode, errorBody, currentPage));
                        break;
                    }

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JArray results = (JArray)JObject.Parse(jsonResponse)["results"];

                    if (results == null || results.Count == 0)
                    {
                        Console.WriteLine("No more games available from API.");
                        break;
                    }

                    foreach (JToken gameJson in results)
                    {
                        RawgGame game = ParseGame(gameJson);
                        if (game == null) continue;
                        allGames.Add(game);
                        gamesRetrieved++;
                        if (gamesRetrieved >= totalGamesToFetch) break;
                    }

                    Console.WriteLine($"Fetched {gamesRetrieved}/{totalGamesToFetch} games (Page {currentPage})");
                    currentPage++;
                    await Task.Delay(100);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Network error (page {currentPage}): {ex.Message}");
                    Console.WriteLine("Check your internet connection and try again.");
                    break;
                }
                catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
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
            return (int)statusCode switch
            {
                401 => "Invalid or missing RAWG API key. Check the 'RawgApiKey' value in App.config.",
                403 => "Access denied by RAWG API. Your key may be restricted or invalid.",
                429 => $"Rate limit exceeded (page {page}). Wait a few minutes before fetching again.",
                500 or 502 or 503 => $"RAWG API server error (page {page}). Try again later.",
                _ => $"API returned {(int)statusCode} (page {page}). {responseBody?.Substring(0, Math.Min(100, responseBody?.Length ?? 0))}"
            };
        }

        // Parse individual game from JSON
        private RawgGame ParseGame(JToken gameJson)
        {
            try
            {
                JArray genres = (JArray)gameJson["genres"];
                JArray publishers = (JArray)gameJson["publishers"];

                return new RawgGame
                {
                    Title = gameJson["name"]?.ToString(),
                    ReleaseDate = gameJson["released"]?.ToString(),
                    Rating = gameJson["rating"]?.ToObject<decimal?>() ?? 0,
                    RatingsCount = gameJson["ratings_count"]?.ToObject<int?>() ?? 0,
                    Metacritic = gameJson["metacritic"]?.ToObject<int?>(),
                    Description = gameJson["description_raw"]?.ToString() ?? "",
                    BackgroundImage = gameJson["background_image"]?.ToString(),
                    Publisher = publishers?.Count > 0 ? publishers[0]["name"]?.ToString() ?? "Unknown" : "Unknown",
                    Genres = genres?.Select(g => g["name"]?.ToString()).Where(g => !string.IsNullOrEmpty(g)).ToList() ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing game: {ex.Message}");
                return null;
            }
        }

        // Simple class to hold game data from API
        public class RawgGame
        {
            public string Title { get; set; }
            public string Publisher { get; set; }
            public string ReleaseDate { get; set; }
            public decimal Rating { get; set; }
            public int RatingsCount { get; set; }
            public int? Metacritic { get; set; }
            public string Description { get; set; }
            public string BackgroundImage { get; set; }
            public List<string> Genres { get; set; }
        }
    }
}