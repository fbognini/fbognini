﻿using fbognini.Core.Data;
using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Application.Persistence
{
    public interface IRepositoryAsync : IDisposable
    {
        #region Create

        Task<T> CreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, IEntity;

        Task<IEnumerable<T>> CreateRangeAsync<T>(IEnumerable<T> entitys, CancellationToken cancellationToken = default) where T : class, IEntity;

        #endregion

        #region Read

        #region GetById

        Task<T> GetByIdAsync<T, TPK>(TPK id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<TPK>
            where TPK : notnull;

        Task<T> GetByIdAsync<T>(int id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<int>;

        Task<T> GetByIdAsync<T>(long id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<long>;

        Task<T> GetByIdAsync<T>(string id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<string>;

        Task<T> GetByIdAsync<T>(Guid id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<Guid>;

        #endregion



        #region GetBySlug

        Task<T> GetBySlugAsync<T>(string slug, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IEntity, IHaveSlug;

        #endregion

        Task<T> GetFirstAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : class, IEntity;
        Task<T> GetLastAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : class, IEntity;
        Task<List<T>> GetAllAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : class, IEntity;
        Task<PaginationResponse<TMapped>> GetSearchResultsAsync<T, TMapped>(SearchCriteria<T> criteria, CancellationToken cancellationToken = default)
            where T : AuditableEntity
            where TMapped : class
            ;

        #endregion

        #region Update

        Task UpdateAsync<T>(T entity) where T : class, IEntity;

        #endregion

        #region Delete

        Task DeleteAsync<T>(T entity) where T : class, IEntity; 
        Task DeleteRangeAsync<T>(IEnumerable<T> entitys) where T : class, IEntity;

        #region DeleteById

        Task<T> DeleteByIdAsync<T, TPK>(TPK id, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<TPK>
            where TPK : notnull
            ;

        Task<T> DeleteByIdAsync<T>(int id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<int>;

        Task<T> DeleteByIdAsync<T>(long id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<long>;

        Task<T> DeleteByIdAsync<T>(string id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<string>;

        Task<T> DeleteByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<Guid>;

        #endregion

        #endregion

        #region UnitOfWork

        Task CreateTransaction(CancellationToken cancellationToken);

        Task<int> Save(CancellationToken cancellationToken);

        Task Commit(CancellationToken cancellationToken);

        Task Rollback(CancellationToken cancellationToken);

        void Detach(IEntity entity);
        void DetachAll();

        #endregion

    }
}
