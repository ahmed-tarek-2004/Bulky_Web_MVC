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
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        public readonly ApplicationDbContext db;
        public ProductImageRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }
        
        public void Update(ProductImage obj)
        {
            db.productImages.Update(obj);
        }
    }
}
