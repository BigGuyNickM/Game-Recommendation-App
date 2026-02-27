using System;

namespace Game_Recommendation.Models
{
    public class User
    {
        public bool IsNewUser { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
