using Bulky_Web.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky_Web.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        [Unique]
        [DisplayName("Category Name")] 
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100,ErrorMessage ="Display Order Must Be '1 : 100' ")] 
        public int DisplayOrder {  get; set; }
    }
}
