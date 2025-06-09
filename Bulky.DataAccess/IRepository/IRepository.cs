using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.IRepository
{
    public interface IRepository <T> where T:class
    {
        public IEnumerable<T> GetAll (string?includeProperties = null); 
        public void Add (T item);
        public T Get(Expression<Func<T,bool>> expression, string? includeProperties = null);
        public void Remove(T entity);
        public void RemoveRange(IEnumerable<T> entity);

    }
}
