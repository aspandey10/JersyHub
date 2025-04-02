 using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Infrastructure.Repo;
using JersyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
 using System.Diagnostics;
using System.Security.Claims;

namespace JersyHub.Areas.Customer.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork uow;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork uow)
        {
            _logger = logger;
            this.uow = uow;
        }

        public IActionResult Index(double? minPrice, double? maxPrice, List<int>? categoryIds)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                HttpContext.Session.SetInt32(StaticDetail.SessionCart,
                    uow.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count());
            }

            // Fetch products and categories
            IEnumerable<Product> products = uow.Product.GetAll(includeProperties: "Category");
            IEnumerable<Category> categories = uow.Category.GetAll();

            // Apply price filtering
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
            }

            // Apply category filtering
            if (categoryIds != null && categoryIds.Count > 0)
            {
                products = products.Where(p => categoryIds.Contains(p.CategoryId));
            }

            ViewBag.Categories = categories; // Pass categories to the view
            ViewBag.MinPrice = minPrice ?? 1000; // Default min price
            ViewBag.MaxPrice = maxPrice ?? 50000; // Default max price
            ViewBag.SelectedCategories = categoryIds ?? new List<int>(); // Preserve selected categories

            return View(products);
        }



        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = uow.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId

            };
            return View(cart);

        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            
            shoppingCart.AddedDate = System.DateTime.Now;

            

            ShoppingCart cartDb = uow.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
            if(cartDb == null)
            { 
                uow.ShoppingCart.Add(shoppingCart);
                HttpContext.Session.SetInt32(StaticDetail.SessionCart, uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
                uow.Save();
            }
            else
            {
                cartDb.Count += shoppingCart.Count;
                uow.ShoppingCart.Update(cartDb);
                uow.Save();

            }
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
