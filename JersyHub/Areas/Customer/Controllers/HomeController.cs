using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
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
        private readonly IShoppingCartService _shoppingcartservice;
        private readonly ICategoryService _categoryservice;
        private readonly IProductsService _productservice;

        public HomeController(ILogger<HomeController> logger,  ICategoryService categoryservice, IProductsService productservice, IShoppingCartService shoppingcartservice)
        {
            _logger = logger;
            _categoryservice = categoryservice;
            _productservice = productservice;
            _shoppingcartservice = shoppingcartservice;
        }

        public IActionResult Index(double? minPrice, double? maxPrice, List<int>? categoryIds)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                HttpContext.Session.SetInt32(StaticDetail.SessionCart,
                    _shoppingcartservice.GetCartsForUser(claim.Value).Count());
            }

            // Fetch products and categories
            IEnumerable<Product> products = _productservice.GetAllProducts();
            IEnumerable<Category> categories = _categoryservice.GetAllCategories();

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
                Product = _productservice.GetProductById(productId),
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



            ShoppingCart cartDb = _shoppingcartservice.GetCart(userId,shoppingCart.ProductId);
            if (cartDb == null)
            {
                _shoppingcartservice.AddToCart(shoppingCart);
                HttpContext.Session.SetInt32(StaticDetail.SessionCart, _shoppingcartservice.GetCartsForUser( userId).Count());
                
            }
            else
            {
                cartDb.Count += shoppingCart.Count;
                _shoppingcartservice.UpdateCart(shoppingCart);

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
























































// using JersyHub.Application.Repository.IRepository;
//using JersyHub.Application.Services.ServiceInterface;
//using JersyHub.Data;
//using JersyHub.Domain.Entities;
//using JersyHub.Infrastructure.Repo;
//using JersyHub.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
// using System.Diagnostics;
//using System.Security.Claims;

//namespace JersyHub.Areas.Customer.Controllers
//{
//    [Area("Customer")]

//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly IShoppingCartService _shoppingcartservice;
//        private readonly IProductsService _productservice;
//        private readonly ICategoryService _categoryservice;

//        public HomeController(ILogger<HomeController> logger,IShoppingCartService shoppingcartservice, IProductsService productservice, ICategoryService categoryservice)
//        {
//            _logger = logger;

//            _shoppingcartservice = shoppingcartservice;
//            _productservice = productservice;
//            _categoryservice = categoryservice;
//        }

//        public IActionResult Index(double? minPrice, double? maxPrice, List<int>? categoryIds)
//        {
//            var claimsIdentity = (ClaimsIdentity)User.Identity;
//            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
//            if (claim != null)
//            {
//                HttpContext.Session.SetInt32(StaticDetail.SessionCart,
//                    _shoppingcartservice.GetCartsForUser(claim.Value).Count());
//            }

//            // Fetch products and categories
//            IEnumerable<Product> products = _productservice.GetAllProducts();
//            IEnumerable<Category> categories = _categoryservice.GetAllCategories();

//            // Apply price filtering
//            if (minPrice.HasValue && maxPrice.HasValue)
//            {
//                products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
//            }

//            // Apply category filtering
//            if (categoryIds != null && categoryIds.Count > 0)
//            {
//                products = products.Where(p => categoryIds.Contains(p.CategoryId));
//            }

//            ViewBag.Categories = categories; // Pass categories to the view
//            ViewBag.MinPrice = minPrice ?? 1000; // Default min price
//            ViewBag.MaxPrice = maxPrice ?? 50000; // Default max price
//            ViewBag.SelectedCategories = categoryIds ?? new List<int>(); // Preserve selected categories

//            return View(products);
//        }



//        public IActionResult Details(int productId)
//        {
//            ShoppingCart cart = new()
//            {
//                Product = _productservice.GetProductById( productId),
//                Count = 1,
//                ProductId = productId

//            };
//            return View(cart);

//        }
//        [HttpPost]
//        [Authorize]
//        public IActionResult Details(ShoppingCart shoppingCart)
//        {
//            var claimsIdentity = (ClaimsIdentity)User.Identity;
//            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
//            shoppingCart.ApplicationUserId = userId;

//            shoppingCart.AddedDate = System.DateTime.Now;



//            ShoppingCart cartDb = _shoppingcartservice.GetCart(userId, shoppingCart.ProductId);
//            if (cartDb == null)
//            { 
//                _shoppingcartservice.AddToCart(shoppingCart);
//                HttpContext.Session.SetInt32(StaticDetail.SessionCart, _shoppingcartservice.GetCartsForUser(userId).Count());

//            }
//            else
//            {
//                cartDb.Count += shoppingCart.Count;
//                _shoppingcartservice.UpdateCart(cartDb);

//            }
//            return RedirectToAction(nameof(Index));

//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}
