namespace ApiTest.Repositories;

[Injectable(InjectLifeTime.Transient,typeof(ICustomeRepository))]
public class CustomeRepository:BaseRepository<CustomeRepository>,ICustomeRepository
{
    
}