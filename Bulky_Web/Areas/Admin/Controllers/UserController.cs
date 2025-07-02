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


        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            return Json(new { success = true, message = "Delete Successful" });
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
                var role = roles.FirstOrDefault(u=>u.Id==uRoles.RoleId);
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
