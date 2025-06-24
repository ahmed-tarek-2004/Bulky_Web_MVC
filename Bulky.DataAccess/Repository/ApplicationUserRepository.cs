using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
