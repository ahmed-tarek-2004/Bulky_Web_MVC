using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API Call
        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader> objHeaderList = unitOfWork.Header.GetAll(includeProperties: "ApplicationUser").ToList();
            return Json(new { data = objHeaderList });
        }
        #endregion
    }
}
