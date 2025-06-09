using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> dbset;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            dbset = _context.Set<T>();
           // _context.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }
        public void Add(T item)
        {
            dbset.Add(item);
        }

        public T Get(Expression<Func<T, bool>> Filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbset.Where(Filter);
            if(!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var include in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include);
                }
            }
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(string ? includeProperties=null)
        {
            IQueryable<T> query = dbset;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var include in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include);
                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity);
        }
    }
}
