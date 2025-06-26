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
    public class DetailRepository : Repository<OrderDetail>, IDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public DetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetail Obj)
        {
            _context.orderDetails.Update(Obj);
        }
    }
}
