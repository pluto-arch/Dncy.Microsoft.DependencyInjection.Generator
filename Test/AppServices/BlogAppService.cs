

using Microsoft.Extensions.DependencyInjection;

namespace AppServices;

[Injectable(InjectLifeTime.Scoped,typeof(IBlogAppService))]
public class BlogAppService:IBlogAppService
{
    /// <inheritdoc />
    public List<string> GetList()
    {
        return Enumerable.Range(0,200).Select(x=>x.ToString()).ToList();
    }
}