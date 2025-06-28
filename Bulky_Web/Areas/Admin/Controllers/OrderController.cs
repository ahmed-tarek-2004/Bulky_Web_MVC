using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Utility;
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
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> objHeaderList = unitOfWork.Header.GetAll(includeProperties: "ApplicationUser").ToList();
            switch (status) {
                case "pending":
                objHeaderList = objHeaderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment|| u.PaymentStatus == SD.PaymentStatusPending).ToList();
                break;
            case "inprocess":
                objHeaderList = objHeaderList.Where(u => u.OrderStatus == SD.StatusInProcess).ToList();
                break;
            case "completed":
                objHeaderList = objHeaderList.Where(u => u.OrderStatus == SD.StatusShipped).ToList();
                break;
            case "approved":
                objHeaderList = objHeaderList.Where(u => u.OrderStatus == SD.StatusApproved).ToList();
                break;
            default:
                break;

            }
            return Json(new { data = objHeaderList });
        }
        #endregion
    }
}
