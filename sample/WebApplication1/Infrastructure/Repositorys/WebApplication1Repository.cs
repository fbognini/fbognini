using fbognini.Infrastructure.Repositorys;
using WebApplication1.Infrastructure.Persistance;

namespace WebApplication1.Infrastructure.Repositorys
{
    public class WebApplication1Repository : RepositoryAsync<WebApplication1DbContext>, IWebApplication1Repository
    {
        public WebApplication1Repository(WebApplication1DbContext context, ILogger<WebApplication1Repository> logger)
            : base(context, logger)
        {

        }
    }
}
