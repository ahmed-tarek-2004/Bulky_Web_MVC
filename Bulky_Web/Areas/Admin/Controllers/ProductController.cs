using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        [ActionName("Create")]
        public IActionResult CreateProduct(Product product)
        {
            var products = IunitOfWork.Product.Get(p => p.Title == product.Title);

            if (products is not null)
            {
                ModelState.AddModelError("Name", "Name Must be Unique");
            }
            if (ModelState.IsValid)
            {
                IunitOfWork.Product.Add(product);
                IunitOfWork.Save();
                TempData["Success"] = "Category Added Successfully";
                return RedirectToAction("Index");
            }
            return View("Create", product);
        }
        public IActionResult Edit(int? Id)
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
            return View("Edit", product);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {

            var products = IunitOfWork.Product.Get(p => p.Title == product.Title && p.Id != product.Id);

            if (products is not null)
            {
                ModelState.AddModelError("Title", "Title Must be Unique");
                return View(products);
            }
            if (ModelState.IsValid)
            {
                IunitOfWork.Product.Update(product);
                IunitOfWork.Save();
                TempData["Success"] = "Category Edit Successfully";
            }
            return RedirectToAction("Index");
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
