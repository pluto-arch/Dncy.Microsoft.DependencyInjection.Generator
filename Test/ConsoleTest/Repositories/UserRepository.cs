using Microsoft.Extensions.DependencyInjection;

namespace ConsoleTest.Repositories;


[Injectable(InjectLifeTime.Transient, typeof(IUserRepository))]
public class UserRepository:IUserRepository
{
    /// <inheritdoc />
    public string Get()
    {
        return "Hello UserRepository";
    }
}