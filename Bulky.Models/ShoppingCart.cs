using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
        public int ProductId { get; set; }
        [Range(1, 1000, ErrorMessage = "Enter Value From 1 To 1000")]
        public int count { get; set; }
        [ValidateNever]
        public ApplicationUser ApplicationUser{get ; set;}
        public string ApplicationUserID { get ; set;}


    }
}
