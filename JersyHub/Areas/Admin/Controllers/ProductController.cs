using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Application.ViewModel;
using JersyHub.Data;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;

namespace JersyHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class ProductController : Controller
    {
        public IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAppEmailSender _emailSender;
        private readonly IProductsService _productservice;
        private readonly ICategoryService _categoryservice;
        private readonly IShoppingCartService _shoppingcartservice;

        public ProductController( IWebHostEnvironment webHostEnvironment,IAppEmailSender emailSender, IProductsService productservice, ICategoryService categoryservice, IShoppingCartService shoppingcartservice)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _productservice = productservice;
            _categoryservice = categoryservice;
            _shoppingcartservice = shoppingcartservice;
        }


        public IActionResult Index()
        {
            var cat_data = _productservice.GetAllProducts();
            
            return View(cat_data);

        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _categoryservice.GetAllCategories().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            ProductVM productVM = new() 
            {
                CategoryList = CategoryList,
                Product = new Product()
            };
            if(id==null || id == 0)
            {
                return View(productVM);

            }
            else
            {
                productVM.Product= _productservice.GetProductById((int)id);
                return View(productVM);
            }

        }
        [HttpPost]
        public async Task<IActionResult> UpsertAsync(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName= Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath= Path.Combine(wwwRootPath, @"images\product");
                    using(var fileStream= new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                      
                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (obj.Product.DiscountPercent > 0)
                {
                    var shoppingCarts = _shoppingcartservice.GetAllCarts();
                    foreach (var cart in shoppingCarts)
                    {
                        if (cart.ProductId == obj.Product.Id)
                        {
                            _productservice.SendEmailToUserAsync(User);
                            cart.LastEmail = DateTime.Now;
                            _uow.ShoppingCart.Update(cart);
                            _uow.Save();
                        }
                    }
                }

                if (obj.Product.Id == 0)
                {
                    _productservice.AddProduct(obj.Product);
                    TempData["success"] = "Product Created Successfully. ";

                }
                else
                {
                    _productservice.UpdateProduct(obj.Product);
                    TempData["success"] = "Product updated Successfully. ";

                }
                
                return RedirectToAction("Index");
            }
            return View();

        }
       

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var data = _productservice.GetProductById((int)id);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }
        [HttpPost]
        public IActionResult Delete(Product obj)
        {
            if (obj == null)
            {
                return NotFound();
            }
            _productservice.DeleteProduct(obj);
            TempData["delete"] = "Product deleted successfully. ";
            return RedirectToAction("Index");

        }
    }
}
