﻿using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.IRepository
{
    public interface IShoppingCartRepository:IRepository<ShoppingCart>
    {
        public void Update(ShoppingCart cart);
    }
}
