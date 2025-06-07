using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class ProductVM
    {
        public Product product { get; set; }
        
        [ValidateNever]// and Also Solved it by making input hidden and pass same values for select List item
        public IEnumerable<SelectListItem> Selects { get; set; }
    }
}
