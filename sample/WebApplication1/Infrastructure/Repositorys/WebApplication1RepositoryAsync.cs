using AutoMapper;
using fbognini.Infrastructure.Repositorys;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using WebApplication1.Application.Interfaces.Repositorys;
using WebApplication1.Infrastructure.Persistance;
using System.Collections;
using fbognini.Application.Persistence;

namespace WebApplication1.Infrastructure.Repositorys
{
    public class WebApplication1RepositoryAsync : RepositoryAsync<WebApplication1DbContext>, IWebApplication1RepositoryAsync
    {
        public WebApplication1RepositoryAsync(WebApplication1DbContext context, IMapper mapper, ILogger<WebApplication1RepositoryAsync> logger)
            : base(context, mapper, logger)
        {

        }
    }
}
