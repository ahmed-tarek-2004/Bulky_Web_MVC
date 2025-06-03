using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _Icategory;
        public CategoryController(ICategoryRepository Icategory)
        {
            _Icategory = Icategory;
        }
        public IActionResult Index()
        {
            var items = _Icategory.GetAll().OrderBy(p => p.DisplayOrder).ToList();
            return View("index", items);
        }
        public IActionResult Create()
        {

            return View("Create");
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name And Display Order Must not be same");
            }
            if (ModelState.IsValid)
            {
                _Icategory.Add(category);
                _Icategory.Save();
                TempData["Success"] = "Category Added Successfully";
                return RedirectToAction("Index");
            }
            return View("create", category);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _Icategory.Get(p=>p.Id==id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name And Display Order Must not be same");
            }
            if (ModelState.IsValid)
            {
                _Icategory.Update(category);
                _Icategory.Save();
            }

            TempData["Success"] = "Category Edited Successfully";
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _Icategory.Get(p => p.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteCategory(int? id)
        {

            Category? category = _Icategory.Get(p => p.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _Icategory.Remove(category);
            _Icategory.Save();
            //context
            TempData["Success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");
        }
        public IActionResult sUnique(string name)
        {
            var exists = _Icategory.GetAll().Any(c => c.Name == name);
            if (exists)
            {
                return Json($"The category name '{name}' is already taken.");
            }

            return Json(true);
        }
    }

}