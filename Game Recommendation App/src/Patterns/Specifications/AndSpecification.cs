using System.Collections.Generic;
using System.Linq;

namespace Game_Recommendation.Patterns.Specifications
{
    public class AndSpecification<T> : ISpecification<T>
    {
        private readonly List<ISpecification<T>> _specifications;

        public AndSpecification(IEnumerable<ISpecification<T>> specifications)
        {
            _specifications = new List<ISpecification<T>>(specifications);
        }

        // All specs must pass for the item to match
        public bool IsSatisfiedBy(T item) =>
            _specifications.All(spec => spec.IsSatisfiedBy(item));
    }
}