using Bulky_Web.Data;
using System.ComponentModel.DataAnnotations;

namespace Bulky_Web.Validation
{
    public class UniqueAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
            if (value == null) return null;
            var catName = dbContext.Categories.Select(b => b.Name == value).FirstOrDefault();
            if (catName == true)
            {
                return new ValidationResult("Category Name Already Exsists");
            }
            
            return ValidationResult.Success;
        }
    }
}
