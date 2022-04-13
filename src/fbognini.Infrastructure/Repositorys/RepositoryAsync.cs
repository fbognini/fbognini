using AutoMapper;
using fbognini.Application.DbContexts;
using fbognini.Application.Persistence;
using fbognini.Application.Utilities;
using fbognini.Core.Data;
using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using fbognini.Infrastructure.Persistence;
using fbognini.Infrastructure.Utilities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Repositorys
{

    public class RepositoryAsync<TContext> : IRepositoryAsync
        where TContext : DbContext
    {
        private bool disposed;
        private readonly TContext context;
        private readonly IDistributedCache cache;
        private readonly IMapper mapper;
        private IDbContextTransaction transaction;

        public RepositoryAsync(TContext dbContext, IDistributedCache cache, IMapper mapper)
        {
            context = dbContext;
            this.cache = cache;
            this.mapper = mapper;
        }

        #region Create

        public async Task<T> CreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : AuditableEntity
        {
            await context.Set<T>().AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<T>> CreateRangeAsync<T>(IEnumerable<T> entitys, CancellationToken cancellationToken = default) where T : AuditableEntity
        {
            await context.Set<T>().AddRangeAsync(entitys, cancellationToken);
            return entitys;
        }

        #endregion

        #region Read

        #region GetById

        public async Task<T> GetByIdAsync<T, TPK>(TPK id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : AuditableEntityWithIdentity<TPK>
            where TPK : notnull
        {
            var query = context.Set<T>().IncludeViews(args);
            if (args != null && !args.Track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task<T> GetByIdAsync<T>(int id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<int>
        {
            return await GetByIdAsync<T, int>(id, args, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(long id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<long>
        {
            return await GetByIdAsync<T, long>(id, args, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(string id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<string>
        {
            return await GetByIdAsync<T, string>(id, args, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(Guid id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<Guid>
        {
            return await GetByIdAsync<T, Guid>(id, args, cancellationToken);
        }

        #endregion

        #region GetBySlug

        public async Task<T> GetBySlugAsync<T>(string slug, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : AuditableEntity, IHaveSlug
        {
            var query = context.Set<T>().IncludeViews(args);
            if (args != null && !args.Track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(x => x.Slug.Equals(slug));
        }

        #endregion

        private IQueryable<T> GetQueryable<T>(SelectCriteria<T> criteria = null) where T : AuditableEntity
        {
            IQueryable<T> query = context.Set<T>();

            if (criteria == null)
            {
                return query;
            }

            if (!criteria.Track) query = query.AsNoTracking();

            query = query
                    .Where(criteria.ResolveFilter().Expand())
                    .AdvancedSearch(criteria)
                    .OrderByDynamic(criteria)
                    .IncludeViews(criteria);

            return query;
        }

        public async Task<T> GetFirstAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : AuditableEntity
        {
            return await GetQueryable(criteria).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<T> GetLastAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : AuditableEntity
        {
            return await GetQueryable(criteria).LastOrDefaultAsync(cancellationToken);
        }

        public async Task<List<T>> GetAllAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : AuditableEntity
        {
            return await GetQueryable(criteria).ToListAsync(cancellationToken);
        }

        public async Task<PaginationResponse<TMapped>> GetSearchResultsAsync<T, TMapped>(SearchCriteria<T> criteria, CancellationToken cancellationToken = default)
            where T : AuditableEntity
            where TMapped: class
        {
            var query = GetQueryable(criteria)
                .QueryPagination(criteria, out var pagination);

            var list = await query.ToListAsync(cancellationToken);
            var response = new PaginationResponse<TMapped>()
            {
                Pagination = pagination,
                Response = mapper.Map<List<TMapped>>(list)
            };

            return response;
        }

        #endregion

        #region Update

        public Task UpdateAsync<T>(T entity) where T : AuditableEntity
        {
            context.Entry(entity).State = EntityState.Modified;
            cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(entity)));
            return Task.CompletedTask;
        }

        #endregion

        #region Delete

        public Task DeleteAsync<T>(T entity) where T : AuditableEntity
        {
            context.Set<T>().Remove(entity);
            cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(entity)));
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync<T>(IEnumerable<T> entitys) where T : AuditableEntity
        {
            context.Set<T>().RemoveRange(entitys);
            foreach (var item in entitys)
            {
                cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(item)));
            }
            return Task.CompletedTask;
        }

        #region DeleteById

        public async Task<T> DeleteByIdAsync<T, TPK>(TPK id, CancellationToken cancellationToken = default)
            where T : AuditableEntityWithIdentity<TPK>
            where TPK : notnull
        {
            var entity = await GetByIdAsync<T, TPK>(id, cancellationToken: cancellationToken);
            await DeleteAsync(entity);
            return entity;
        }

        public async Task<T> DeleteByIdAsync<T>(int id, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<int>
        {
            return await DeleteByIdAsync<T, int>(id, cancellationToken);
        }

        public async Task<T> DeleteByIdAsync<T>(long id, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<long>
        {
            return await DeleteByIdAsync<T, long>(id, cancellationToken);
        }

        public async Task<T> DeleteByIdAsync<T>(string id, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<string>
        {
            return await DeleteByIdAsync<T, string>(id, cancellationToken);
        }

        public async Task<T> DeleteByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : AuditableEntityWithIdentity<Guid>
        {
            return await DeleteByIdAsync<T, Guid>(id, cancellationToken);
        }

        #endregion

        #endregion

        #region UnitOfWork

        public async Task CreateTransaction(CancellationToken cancellationToken)
        {
            transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<int> Save(CancellationToken cancellationToken)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }

        public Task Commit(CancellationToken cancellationToken)
        {
            transaction.CommitAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public Task Rollback(CancellationToken cancellationToke)
        {
            context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            if (transaction != null)
            {
                transaction.RollbackAsync(cancellationToke);
                transaction.Dispose();
            }

            return Task.CompletedTask;
        }

        #endregion
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose managed resources

                    context.Dispose();
                }
            }
            //dispose unmanaged resources
            disposed = true;
        }
    }
}
