using Bulky_Web.Data;
using Bulky_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bulky_Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext context;
        public CategoryController(ApplicationDbContext _context)
        {
            context = _context;
        }
        public IActionResult Index()
        {
            var items = context.Categories.OrderBy(p => p.DisplayOrder).ToList();
            return View("index", items);
        }
        public IActionResult Create()
        {

            return View("Create");
        }
        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            if (category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name And Display Order Must not be same");
            }
            if (ModelState.IsValid)
            {
                context.Add(category);
                context.SaveChanges();
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
            var category = context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            if (category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name And Display Order Must not be same");
            }
            if (ModelState.IsValid)
            {
                context.Update(category);
                context.SaveChanges();
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
            var category = context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost]
        public IActionResult DeleteCategory(int? id)
        {

            Category? category = context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            context.Remove(category);
            context.SaveChanges();

            TempData["Success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
