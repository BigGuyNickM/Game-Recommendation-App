using Game_Recommendation.Cli.Config;
using Game_Recommendation.Models;
using Game_Recommendation.Specifications;

namespace Game_Recommendation.Specifications
{
    public class KeywordSpecification : ISpecification<Game>
    {
        private readonly string _keyword;
        public KeywordSpecification(string keyword)
        {
            _keyword = keyword.ToLower().Trim();
        }
        public bool IsSatisfiedBy(Game game)
        {
            string title = game.Title.ToLower();

            if (title.Contains(_keyword))
                return true;

            return _FuzzyMatch(title, _keyword);
        }

        // --- Private helpers ---

        private static bool _FuzzyMatch(string title, string keyword)
        {
            string[] words = title.Split(' ');
            foreach (string word in words)
            {
                if (_LevenshteinDistance(word, keyword) <= AppConfig.FuzzyMatchTolerance)
                    return true;
            }
            return false;
        }

        private static int _LevenshteinDistance(string a, string b)
        {
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;

            int[,] matrix = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) matrix[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) matrix[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    matrix[i, j] = System.Math.Min(
                        System.Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost
                    );
                }
            }

            return matrix[a.Length, b.Length];
        }
    }
}
