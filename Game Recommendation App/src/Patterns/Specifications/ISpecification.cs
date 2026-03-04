namespace Game_Recommendation.Patterns.Specifications
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
    }
}
