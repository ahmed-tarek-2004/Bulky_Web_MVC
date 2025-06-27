using BulkyBook.DataAccess.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCartVM shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCarts = unitOfWork.ShoppingCart.GetAll(b => b.ApplicationUserID == userId, includeProperties: "Product").ToList(),
                orderHeaders = new()
            };
            foreach (var cart in shoppingCartVM.ShoppingCarts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.orderHeaders.OrderTotal += (cart.Price * cart.count);
            }

            return View(shoppingCartVM);
        }

        public IActionResult Plus(int Id)
        {
            var cart = unitOfWork.ShoppingCart.Get(b => b.Id == Id);
            cart.count += 1;
            unitOfWork.ShoppingCart.Update(cart);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int Id)
        {
            var cart = unitOfWork.ShoppingCart.Get(b => b.Id == Id);
            if (cart.count <= 1)
            {
                unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                cart.count -= 1;
                unitOfWork.ShoppingCart.Update(cart);
            }
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int Id)
        {
            var cart = unitOfWork.ShoppingCart.Get(b => b.Id == Id);
            unitOfWork.ShoppingCart.Remove(cart);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCartVM = new()
            {
                ShoppingCarts = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId,
                includeProperties: "Product"),
                orderHeaders = new()
            };
            ShoppingCartVM.orderHeaders.ApplicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.orderHeaders.Name = ShoppingCartVM.orderHeaders.ApplicationUser.Name;
            ShoppingCartVM.orderHeaders.PhoneNumber = ShoppingCartVM.orderHeaders.ApplicationUser.PhoneNumber;
            ShoppingCartVM.orderHeaders.StreetAddress = ShoppingCartVM.orderHeaders.ApplicationUser.StreetAddress;
            ShoppingCartVM.orderHeaders.City = ShoppingCartVM.orderHeaders.ApplicationUser.City;
            ShoppingCartVM.orderHeaders.State = ShoppingCartVM.orderHeaders.ApplicationUser.State;
            ShoppingCartVM.orderHeaders.PostalCode = ShoppingCartVM.orderHeaders.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCarts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeaders.OrderTotal += (cart.Price * cart.count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCartVM.ShoppingCarts = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserID == userId,
                includeProperties: "Product");

            ShoppingCartVM.orderHeaders.OrderDate = DateTime.Now;
            ApplicationUser ApplicationUser = unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.orderHeaders.ApplicationUserId = userId;


            foreach (var cart in ShoppingCartVM.ShoppingCarts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeaders.OrderTotal += (cart.Price * cart.count);
            }

            if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.orderHeaders.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.orderHeaders.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.orderHeaders.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.orderHeaders.OrderStatus = SD.StatusApproved;
            }
            unitOfWork.Header.Add(ShoppingCartVM.orderHeaders);
            unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCarts)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.orderHeaders.Id,
                    Price = cart.Price,
                    Count = cart.count
                };
                unitOfWork.Details.Add(orderDetail);
            }
            if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                //stripe logic
                var domain = "https://localhost:5182/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.orderHeaders.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCarts)
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
                        Quantity = item.count
                    };
                    options.LineItems.Add(sessionLineItem);
                }


                var service = new SessionService();
                Session session = service.Create(options);
                unitOfWork.Header.UpdateStripePaymentID(ShoppingCartVM.orderHeaders.Id, session.Id, session.PaymentIntentId);
                unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            unitOfWork.Save();
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.orderHeaders.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = unitOfWork.Header.Get(b => b.Id == id, includeProperties: "ApplicationUSer");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var Service = new SessionService();
                var Session = Service.Get(orderHeader.SessionId);
                if (Session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.Header.UpdateStripePaymentID(id, Session.Id, Session.PaymentIntentId);
                    unitOfWork.Header.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserID == orderHeader.ApplicationUserId).ToList();

            unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            unitOfWork.Save();
            return View(id);
        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {

            if (shoppingCart.count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
