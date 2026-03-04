using Game_Recommendation.Models;
using Game_Recommendation.Database.Repositories;
using Game_Recommendation.Patterns.Specifications;
using System.Collections.Generic;
using System.Linq;

namespace Game_Recommendation.Services
{
    public class SearchService
    {
        private readonly GameRepository _gameRepo;

        public SearchService(GameRepository gameRepo)
        {
            _gameRepo = gameRepo;
        }

        // Filters by genre first (cheaper db query), then applies keyword specs in memory
        public List<Game> Search(List<int> genreIds, List<string> keywords)
        {
            var games = genreIds.Count > 0
                ? _gameRepo.GetGamesByGenres(genreIds)
                : _gameRepo.GetAllGames();

            if (keywords.Count == 0) return games;

            var specs = keywords.Select(k => (ISpecification<Game>)new KeywordSpecification(k)).ToList();
            var filter = new AndSpecification<Game>(specs);
            return games.Where(g => filter.IsSatisfiedBy(g)).ToList();
        }
    }
}