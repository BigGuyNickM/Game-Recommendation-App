namespace Game_Recommendation.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string GameDescription { get; set; }
        public decimal? AvgRating { get; set; }
        public int? TotalRatings { get; set; }
    }
}