namespace ApiTest.Repositories;

[Injectable(InjectLifeTime.Transient,typeof(IBaseRepository<>))]
public class BaseRepository<T>:IBaseRepository<T>
{
    /// <inheritdoc />
    public IEnumerable<string> GetList()
    {
        foreach (var item in Enumerable.Range(1,200))
        {
            yield return $"at : {item}";
        }
    }
}