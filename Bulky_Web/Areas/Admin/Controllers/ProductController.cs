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
    [Authorize(Roles = SD.Role_Admin)]
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
            productvm.product = IunitOfWork.Product.Get(p => p.Id == id, includeProperties: "ProductImages");
            return View(productvm);
        }

        [HttpPost]
        [ActionName("UpSert")]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (productVM.product.Id == 0)
                {
                    IunitOfWork.Product.Add(productVM.product);
                }
                else
                {
                    IunitOfWork.Product.Update(productVM.product);
                }

                IunitOfWork.Save();


                string wwwRootPath = IWebHostEnvironment.WebRootPath;
                if (files != null)
                {

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.product.Id,
                        };

                        if (productVM.product.ProductImages == null)
                            productVM.product.ProductImages = new List<ProductImage>();

                        productVM.product.ProductImages.Add(productImage);

                    }

                    IunitOfWork.Product.Update(productVM.product);
                    IunitOfWork.Save();
                }

                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.Selects = IunitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
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
            var path = @"images/products/product-" + Id;
            path = Path.Combine(IWebHostEnvironment.WebRootPath, path);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);// true=> delte folde with all it's content
                
                // instead of 

                /*if (Directory.Exists(path))
                {
                    string[] filePaths = Directory.GetFiles(path);
                    foreach (string filePath in filePaths)
                    {
                        System.IO.File.Delete(filePath);
                    }

                    Directory.Delete(path);
                }*/

            }
            IunitOfWork.Product.Remove(productToBeDeleted);
            IunitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = IunitOfWork.ProductImages.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(IWebHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                IunitOfWork.ProductImages.Remove(imageToBeDeleted);
                IunitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
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
