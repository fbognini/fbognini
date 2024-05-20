using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Repository
{
    public interface IRepositoryAsyncFactory<TRepositoryAsync>
        where TRepositoryAsync : IRepositoryAsync
    {
        TRepositoryAsync CreateNewRepository();
    }
}
