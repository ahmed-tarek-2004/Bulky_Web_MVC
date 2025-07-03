using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {

        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        public UserController(ApplicationDbContext _context, UserManager<IdentityUser> userManager)
        {
            context = _context;
            this.userManager = userManager;
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
        public IActionResult RoleManagment(string userId)
        {

            string RoleID = context.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

            RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
                ApplicationUser = context.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
                RoleList = context.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }).ToList(),
                CompanyList = context.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }).ToList()
            };

            RoleVM.ApplicationUser.Role = context.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
            return View(RoleVM);
        }
        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {

            string RoleID = context.UserRoles.FirstOrDefault(u => u.UserId == roleManagmentVM.ApplicationUser.Id).RoleId;
            string oldRole = context.Roles.FirstOrDefault(u => u.Id == RoleID).Name;

            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {
                ApplicationUser applicationUser = context.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.ApplicationUser.Id);
                if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                context.SaveChanges();

                userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();

            }

            return RedirectToAction("Index");
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
