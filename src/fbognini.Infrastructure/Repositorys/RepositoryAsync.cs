using AutoMapper;
using EFCore.BulkExtensions;
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
using Snickler.EFCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public T Create<T>(T entity) where T : class, IEntity
        {
            context.Set<T>().Add(entity);
            return entity;
        }
        public async Task<T> CreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            await context.Set<T>().AddAsync(entity, cancellationToken);
            return entity;
        }

        public IEnumerable<T> CreateRange<T>(IEnumerable<T> entitys) where T : class, IEntity
        {
            context.Set<T>().AddRange(entitys);
            return entitys;
        }

        public async Task<IEnumerable<T>> CreateRangeAsync<T>(IEnumerable<T> entitys, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            await context.Set<T>().AddRangeAsync(entitys, cancellationToken);
            return entitys;
        }

        public async Task MassiveInsertAsync<T>(IList<T> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            if (entities.First() is AuditableEntity)
            {
                foreach (var entry in entities as IList<AuditableEntity>)
                {
                    entry.CreatedBy = null;
                    entry.Created = DateTime.Now;
                    entry.LastUpdatedBy = null;
                    entry.LastUpdated = DateTime.Now;
                }
            }

            await context.BulkInsertAsync(entities, bulkConfig, cancellationToken: cancellationToken);
        }

        #endregion

        #region Read

        #region GetById

        public async Task<T> GetByIdAsync<T, TPK>(TPK id, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<TPK>
            where TPK : notnull
        {
            var query = context.Set<T>().IncludeViews(args);
            if (args != null && !args.Track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task<T> GetByIdAsync<T>(int id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : class, IHasIdentity<int>
        {
            return await GetByIdAsync<T, int>(id, args, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(long id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : class, IHasIdentity<long>
        {
            return await GetByIdAsync<T, long>(id, args, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(string id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : class, IHasIdentity<string>
        {
            return await GetByIdAsync<T, string>(id, args, cancellationToken);
        }

        public async Task<T> GetByIdAsync<T>(Guid id, SelectArgs<T> args = null, CancellationToken cancellationToken = default) where T : class, IHasIdentity<Guid>
        {
            return await GetByIdAsync<T, Guid>(id, args, cancellationToken);
        }

        #endregion

        #region GetBySlug

        public async Task<T> GetBySlugAsync<T>(string slug, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IEntity, IHaveSlug
        {
            var query = context.Set<T>().IncludeViews(args);
            if (args != null && !args.Track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(x => x.Slug.Equals(slug));
        }

        #endregion

        #region GetByName

        public async Task<T> GetByNameAsync<T>(string name, SelectArgs<T> args = null, CancellationToken cancellationToken = default)
            where T : class, IEntity, IHaveName
        {
            var query = context.Set<T>().IncludeViews(args);
            if (args != null && !args.Track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(x => x.Name.Equals(name));
        }

        #endregion

        private IQueryable<T> GetQueryable<T>(SelectCriteria<T> criteria = null) where T : class, IEntity
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

        public async Task<T> GetFirstAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetQueryable(criteria).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<T> GetLastAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            return await GetQueryable(criteria).LastOrDefaultAsync(cancellationToken);
        }

        public async Task<List<T>> GetAllAsync<T>(SelectCriteria<T> criteria = null, CancellationToken cancellationToken = default) where T : class, IEntity
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

        public void Update<T>(T entity) where T : class, IEntity
        {
            context.Entry(entity).State = EntityState.Modified;
            cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(entity)));
        }

        public async Task MassiveUpdateAsync<T>(IList<T> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            if (entities.First() is AuditableEntity)
            {
                foreach (var entry in entities as IList<AuditableEntity>)
                {
                    cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(entry)));

                    entry.LastModifiedBy = null;
                    entry.LastModified = DateTime.Now;
                    entry.LastUpdatedBy = null;
                    entry.LastUpdated = DateTime.Now;
                }
            }

            await context.BulkUpdateAsync(entities, bulkConfig, cancellationToken: cancellationToken);
        }


        #endregion

        #region Delete

        public void Delete<T>(T entity) where T : class, IEntity
        {
            context.Set<T>().Remove(entity);
            cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(entity)));
        }

        public void DeleteRange<T>(SelectCriteria<T> criteria) where T : class, IEntity
        {
            var entities = GetQueryable(criteria);
            DeleteRange(entities);
        }

        public void DeleteRange<T>(IEnumerable<T> entities) where T : class, IEntity
        {
            context.Set<T>().RemoveRange(entities);
            foreach (var item in entities)
            {
                cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(item)));
            }
        }

        public async Task BatchDeleteAsync<T>(SelectCriteria<T> criteria, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            var entities = GetQueryable(criteria);
            await entities.BatchDeleteAsync(cancellationToken);
            foreach (var item in entities)
            {
                cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(item)));
            }
        }

        public async Task MassiveDeleteAsync<T>(IList<T> entities, BulkConfig bulkConfig = null, CancellationToken cancellationToken = default) where T : class, IEntity
        {
            if (entities.First() is ISoftDelete)
            {
                foreach (var entry in entities as IList<ISoftDelete>)
                {
                    cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(entry)));

                    entry.DeletedBy = null;
                    entry.Deleted = DateTime.Now;
                }

                await context.BulkUpdateAsync(entities, bulkConfig, cancellationToken: cancellationToken);
                return;
            }

            await context.BulkDeleteAsync(entities, bulkConfig, cancellationToken: cancellationToken);
            foreach (var item in entities)
            {
                cache.Remove(CacheKeys.GetCacheKey<T>(context.FindPrimaryKeyValues(item)));
            }
        }

        #region DeleteById

        public async Task<T> DeleteByIdAsync<T, TPK>(TPK id, CancellationToken cancellationToken = default)
            where T : class, IHasIdentity<TPK>
            where TPK : notnull
        {
            var entity = await GetByIdAsync<T, TPK>(id, cancellationToken: cancellationToken);
            Delete(entity);
            return entity;
        }

        public async Task<T> DeleteByIdAsync<T>(int id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<int>
        {
            return await DeleteByIdAsync<T, int>(id, cancellationToken);
        }

        public async Task<T> DeleteByIdAsync<T>(long id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<long>
        {
            return await DeleteByIdAsync<T, long>(id, cancellationToken);
        }

        public async Task<T> DeleteByIdAsync<T>(string id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<string>
        {
            return await DeleteByIdAsync<T, string>(id, cancellationToken);
        }

        public async Task<T> DeleteByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : class, IHasIdentity<Guid>
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

        public async Task Commit(CancellationToken cancellationToken)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task Rollback(CancellationToken cancellationToke)
        {
            context.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToke);
                transaction.Dispose();
            }
        }

        public void Detach(IEntity entity)
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        public void DetachAll()
        {
            context.ChangeTracker.Clear();
        }

        #endregion

        public DbCommand LoadStoredProcedure(string name, bool prependDefaultSchema = true, short commandTimeout = 30)
        {
            return context.LoadStoredProc(name, prependDefaultSchema, commandTimeout);
        }

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
