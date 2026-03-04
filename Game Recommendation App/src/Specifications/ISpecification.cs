namespace Game_Recommendation.Specifications
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
    }
}
