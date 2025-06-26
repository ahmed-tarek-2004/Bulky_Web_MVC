using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class HeaderRepository : Repository<OrderHeader>, IHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public HeaderRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
        public void Update(OrderHeader Objs)
        {
           _context.orderHeaders.Update(Objs);
        }
    }
}
