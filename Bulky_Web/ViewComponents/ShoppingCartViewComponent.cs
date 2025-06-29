using BulkyBook.DataAccess.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim is not null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) is null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(s => s.ApplicationUserID == claim.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }

            HttpContext.Session.Clear();
            return View(0);
        }
    }
}
