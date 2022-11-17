#if NET6_0 || NETSTANDARD2_1
using EFCore.BulkExtensions;
#endif
using fbognini.Application.Entities;
using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence
{
    public interface IBaseDbContext
    {
        DbSet<Audit> AuditTrails { get; set; }
        public string UserId { get; }
        public string Tenant { get; }
        public string ConnectionString { get; }
    }
}
