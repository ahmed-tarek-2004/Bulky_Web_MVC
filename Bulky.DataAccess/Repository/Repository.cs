using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using Microsoft.EntityFrameworkCore;

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
        }
        public void Add(T item)
        {
            dbset.Add(item);
        }

        public T Get(Expression<Func<T, bool>> Filter)
        {
            return dbset.Where(Filter).FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            return dbset.ToList();
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
