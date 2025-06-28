using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
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
        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new OrderVM()
            {
                orderHeader = unitOfWork.Header.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                orderDetail = [.. unitOfWork.Details.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")]
            };
            return View(orderVM);
        }

        #region API Call
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> objHeaderList = unitOfWork.Header.GetAll(includeProperties: "ApplicationUser").ToList();
            switch (status)
            {
                case "pending":
                    objHeaderList = objHeaderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment || u.PaymentStatus == SD.PaymentStatusPending).ToList();
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
