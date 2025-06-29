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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork IunitOfWork;
        private readonly IWebHostEnvironment IWebHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment IwebHostEnvironment)
        {
            IunitOfWork = unitOfWork;
            IWebHostEnvironment = IwebHostEnvironment;
        }
        public IActionResult Index()
        {
            var company = IunitOfWork.Company.GetAll().OrderBy(p => p.Name).ToList();
            return View("Index", company);
        }
        public IActionResult UpSert(int? id)
        {

            if (id == 0 || id is null)
            {
                return View("UpSert", new Company());
            }
            Company companyObj = IunitOfWork.Company.Get(p => p.Id == id);
            return View(companyObj);
        }

        [HttpPost]
        [ActionName("UpSert")]
        public IActionResult UpSertCompany(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                if (companyObj.Id == null || companyObj.Id == 0)
                {
                    IunitOfWork.Company.Add(companyObj);
                    TempData["Success"] = "Company Added Successfully";
                }
                else
                {
                    IunitOfWork.Company.Update(companyObj);
                    TempData["Success"] = "Company Updated Successfully";
                }
                IunitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return View(companyObj);
            }

        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            var companyToBeDeleted = IunitOfWork.Company.Get(u => u.Id == Id);
            if (companyToBeDeleted == null)
            {
                return Json(new { sucess = false, message = "Error While Deleting" });
            }
            IunitOfWork.Company.Remove(companyToBeDeleted);
            IunitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = IunitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }
        #endregion

    }
}
