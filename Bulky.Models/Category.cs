using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
//ystem.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
//using Microsoft.AspNetCore.Mvc;
namespace BulkyBook.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        [Display(Name ="Category Name")] 
        [Remote(action: "isUnique", controller: "Category" ,areaName:"Admin", ErrorMessage = "Name Must Be Unique")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100,ErrorMessage ="Display Order Must Be '1 : 100' ")] 
        public int DisplayOrder {  get; set; }
        [JsonIgnore]
        public List<Product> Products { get; set; }=new List<Product>(){ };
    }
}
