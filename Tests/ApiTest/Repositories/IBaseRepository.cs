namespace ApiTest.Repositories;

public interface IBaseRepository<T>
{
    IEnumerable<string> GetList();
}