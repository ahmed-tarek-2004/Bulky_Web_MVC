using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {

        private readonly ApplicationDbContext context;
        public UserController(ApplicationDbContext _context)
        {
            context = _context;
        }
        public IActionResult Index()
        {
            return View("Index");
        }


        [HttpPost]
        public IActionResult LockUnlock([FromBody] string? Id)
        {
            var user = context.ApplicationUsers.FirstOrDefault(b => b.Id == Id);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            user.LockoutEnd = (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now) ? DateTime.Now 
                : DateTime.Now.AddYears(100);
            context.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = context.ApplicationUsers.Include(u => u.Company).ToList();
            var userRole = context.UserRoles.ToList();
            var roles = context.Roles.ToList();
            foreach (var user in objUserList)
            {
                var uRoles = userRole.FirstOrDefault(u => u.UserId == user.Id);
                var role = roles.FirstOrDefault(u => u.Id == uRoles.RoleId);
                user.Role = role.Name;
                if (user is null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = objUserList });
        }
        #endregion


    }
}
