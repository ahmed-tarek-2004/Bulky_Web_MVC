using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart Cart = new ShoppingCart()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                ProductId = productId,
                count = 1
            };
            return View(Cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            //shoppingCart.ApplicationUserID = userId;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = _unitOfWork.ShoppingCart.Get(b => b.ProductId == shoppingCart.ProductId && b.ApplicationUserID == userId );

            if (cart == null)
            {
                shoppingCart.ApplicationUserID = userId;
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                TempData["Success"] = "Cart Added Sucessfully";
            }
            else
            {
                cart.count += shoppingCart.count;
                _unitOfWork.ShoppingCart.Update(cart);
                TempData["Success"] = "Cart Updated Sucessfully"; 
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
