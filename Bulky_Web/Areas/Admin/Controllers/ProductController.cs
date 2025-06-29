using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using static System.Net.Mime.MediaTypeNames;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork IunitOfWork;
        private readonly IWebHostEnvironment IWebHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment IwebHostEnvironment)
        {
            IunitOfWork = unitOfWork;
            IWebHostEnvironment = IwebHostEnvironment;
        }
        public IActionResult Index()
        {
            var product = IunitOfWork.Product.GetAll(includeProperties: "Category").OrderBy(p => p.Title).ToList();
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
            var products = IunitOfWork.Product.Get(p => p.Title == productvm.product.Title && p.Id != productvm.product.Id);

            if (products is not null)
            {
                ModelState.AddModelError("Name", "Name Must be Unique");
            }
            if (ModelState.IsValid)
            {
                string root = IWebHostEnvironment.WebRootPath;
                if (file is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(root, @"images\products");
                    if (!string.IsNullOrEmpty(productvm.product.ImgURL))
                    {
                        string oldPath = Path.Combine(root, productvm.product.ImgURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    using (var fileSystem = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileSystem);
                    }
                    productvm.product.ImgURL = @"\images\products\" + fileName;
                }
                if (productvm.product.Id == null || productvm.product.Id == 0)
                {
                    IunitOfWork.Product.Add(productvm.product);
                    TempData["Success"] = "Product Added Successfully";
                }
                else
                {
                    IunitOfWork.Product.Update(productvm.product);
                    TempData["Success"] = "Product Updated Successfully";
                }
                IunitOfWork.Save();
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
        
       [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            var productToBeDeleted = IunitOfWork.Product.Get(u => u.Id == Id); 
            if (productToBeDeleted == null)
            {
                return Json(new { sucess = false, message = "Error While Deleting" });
            }
            var oldImagePath =
                           Path.Combine(IWebHostEnvironment.WebRootPath,
                           productToBeDeleted.ImgURL.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            IunitOfWork.Product.Remove(productToBeDeleted); 
            IunitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = IunitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }
        #endregion

    }
}
