using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ICategoryRepository Category {get;private set;}
        public IProductRepository Product {get;private set;}
        public ICompanyRepository Company {get;private set;}
        public IShoppingCartRepository ShoppingCart {get;private set;}
        public IApplicationUserRepository ApplicationUser {get;private set;}
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ApplicationUser=new ApplicationUserRepository(context);
            Category = new CategoryRepository(_context);
            Product = new ProductRepository(_context);
            Company = new CompanyRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
