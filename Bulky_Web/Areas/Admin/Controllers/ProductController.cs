using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork IunitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            IunitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var product = IunitOfWork.Product.GetAll().OrderBy(p => p.Title).ToList();
            return View("Index", product);
        }
        public IActionResult UpSert(int? id)
        {
            ProductVM productvm = new()
            {
                Selects = IunitOfWork.Category.GetAll().Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }),
                product = new()
            };
            if (id == 0 || id is null)
            {
                return View("UpSert", productvm);
            }
            productvm.product = IunitOfWork.Product.Get(p => p.Id == id);
            return View(productvm);
        }

        [HttpPost]
        [ActionName("UpSert")]
        public IActionResult UpSertProduct(ProductVM productvm, IFormFile? file)
        {
            var products = IunitOfWork.Product.Get(p => p.Title == productvm.product.Title);

            if (products is not null)
            {
                ModelState.AddModelError("Name", "Name Must be Unique");
            }
            if (ModelState.IsValid)
            {
                IunitOfWork.Product.Add(productvm.product);
                IunitOfWork.Save();
                TempData["Success"] = "Category Added Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productvm.Selects = IunitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productvm);
            }

        }

        public IActionResult Delete(int? Id)
        {
            if (Id == 0 || Id == null)
            {
                return NotFound();
            }
            var product = IunitOfWork.Product.Get(p => p.Id == Id);
            if (product is null)
            {
                return NotFound();
            }
            return View("Delete", product);
        }

        [HttpPost]
        public IActionResult Delete(Product product)
        {
            IunitOfWork.Product.Remove(product);
            IunitOfWork.Save();
            //context
            TempData["Success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
