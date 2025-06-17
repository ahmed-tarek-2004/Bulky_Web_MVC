using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
       
        public void Update(Company obj)
        {
            _context.Update(obj);
        }
    }
}
