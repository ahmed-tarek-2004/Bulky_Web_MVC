using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
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
            OrderVM = new OrderVM()
            {
                orderHeader = unitOfWork.Header.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                orderDetail = [.. unitOfWork.Details.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")]
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = unitOfWork.Header.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.orderHeader.City;
            orderHeaderFromDb.State = OrderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.TrackingNumber;
            }
            unitOfWork.Header.Update(orderHeaderFromDb);
            unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully.";


            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            unitOfWork.Header.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusInProcess);
            unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader = unitOfWork.Header.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            unitOfWork.Header.Update(orderHeader);
            unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {

            var orderHeader = unitOfWork.Header.Get(u => u.Id == OrderVM.orderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                unitOfWork.Header.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                unitOfWork.Header.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });

        }
        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.orderHeader = unitOfWork.Header
                .Get(u => u.Id == OrderVM.orderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.orderDetail = [..unitOfWork.Details
                .GetAll(u => u.OrderHeaderId == OrderVM.orderHeader.Id, includeProperties: "Product")];

            //stripe logic
            var domain = "https://localhost:7169/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            unitOfWork.Header.UpdateStripePaymentID(OrderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = unitOfWork.Header.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.Header.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    unitOfWork.Header.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }


            }


            return View(orderHeaderId);
        }


        #region API Call
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> objHeaderList;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objHeaderList = unitOfWork.Header.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                objHeaderList = unitOfWork.Header.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
            }
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
