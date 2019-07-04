using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using EntityFramework.Extensions;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Data.DB;

namespace GlobalReportingSystem.Data.Concrete
{
    public class RepositoryBase<T> : IRepository<T>, IDisposable
         where T : class
    {
        private DbContext _context;

        public RepositoryBase(DbContext context)
        {
            _context = context;
            _context.Configuration.AutoDetectChangesEnabled = true;
        }

        public virtual IDbSet<T> DbSet
        {
            get { return _context.Set<T>(); }
        }

        #region IRepository Members

        public virtual bool HasChanges
        {
            get { return _context.ChangeTracker.Entries().Any(); }
        }

        public virtual T Add(T entity)
        {
           /* try
            {
                _context.Configuration.AutoDetectChangesEnabled = false;*/
               return DbSet.Add(entity);
          /*  }
            finally
            {
                _context.Configuration.AutoDetectChangesEnabled = true;
            }
            return entity;*/
        }

        public virtual void Delete(T entity)
        {
            DbSet.Remove(entity);
        }
        public virtual void Delete(Expression<Func<T, bool>> whereCondition)
        {
            DbSet.Where(whereCondition).Delete();
        }

        public virtual IList<T> GetAllToList()
        {
            return DbSet.ToList();
        }

        public virtual IList<T> GetAllToList(Expression<Func<T, bool>> whereCondition)
        {
            return DbSet.AsNoTracking().Where(whereCondition).ToList();
        }
        public virtual IQueryable<T>  GetAll(Expression<Func<T, bool>> whereCondition)
        {
            return DbSet.Where(whereCondition);
        }

        public virtual T GetFirstOrDefault(Expression<Func<T, bool>> whereCondition)
        {
            var t = DbSet/*.AsNoTracking()*/.Where(whereCondition);
            return t.FirstOrDefault();
        }
        public virtual T GetFirstOrDefault_Services(Expression<Func<T, bool>> whereCondition)
        {
            var t = DbSet.AsNoTracking().Where(whereCondition);
            return t.FirstOrDefault();
        }

        public virtual T GetLastOrDefault(Expression<Func<T, bool>> whereCondition)
        {
            var t = DbSet/*.AsNoTracking()*/.Where(whereCondition);
            if (t.Count() != 0)
                return t.ToList().Last();
            return null;
        }
        public virtual T GetLastOrDefault_Services(Expression<Func<T, bool>> whereCondition)
        {
            var t = DbSet.AsNoTracking().Where(whereCondition);
            if (t.Count() != 0)
                return t.ToList().Last();
            return null;
        }

        public virtual T GetLast()
        {
            var t = DbSet/*.AsNoTracking()*/.Last();
            return t;
        }

        public virtual T GetSingleOrDefault(Expression<Func<T, bool>> whereCondition)
        {
            return DbSet.Where(whereCondition).SingleOrDefault();
        }

        public virtual T GetSingleOrDefault_Services(Expression<Func<T, bool>> whereCondition)
        {
            return DbSet.AsNoTracking().Where(whereCondition).SingleOrDefault();
        }

        public virtual void Attach(T entity)
        {
            DbSet.Attach(entity);
        }

        public virtual bool Exists(T entity)
        {
            return DbSet.Local.Any(e => e == entity);
        }

        public virtual IQueryable<T> AsQueryable()
        {
            return DbSet.AsQueryable();
        }

        public virtual IQueryable<T> AsQueryable(Expression<Func<T, bool>> whereCondition)
        {
            return DbSet.AsQueryable();
        }

        public virtual long Count()
        {
            return DbSet.LongCount();
        }

        public virtual long Count(Expression<Func<T, bool>> whereCondition)
        {
            return DbSet.Where(whereCondition).LongCount();
        }

        public virtual bool SaveChanges()
        {
            try
            {
                ((IObjectContextAdapter)_context).ObjectContext.CommandTimeout = 1800;
                return 0 < _context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new ApplicationException(sb.ToString());
            }

        }

        public virtual IQueryable<T> GetIncluding(string[] includes)
        {
            IQueryable<T> q = null;
            includes.ToList().ForEach(x => q = q.Include(x));
            return q;
        }
        public virtual IQueryable<T> GetIncluding(string[] includes, Expression<Func<T, bool>> whereCondition)
        {
            var q = DbSet.AsQueryable().Where(whereCondition);
            includes.ToList().ForEach(x => q = q.Include(x));
            return q;
        }
        public virtual IQueryable<T> GetIncludingWithFiltering(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>().Where(filter);
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }
        public virtual IQueryable<T> GetIncludingWithFiltering_Services(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking().Where(filter);
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }
        public IQueryable<T> GetIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        #endregion

    }
}
