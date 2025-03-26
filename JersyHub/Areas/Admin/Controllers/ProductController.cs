using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using JersyHub.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;

namespace JersyHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin)]

    public class ProductController : Controller
    {
        public IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAppEmailSender _emailSender;

        public ProductController(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment,IAppEmailSender emailSender)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }


        public IActionResult Index()
        {
            var cat_data = _uow.Product.GetAll(includeProperties:"Category").ToList();
            
            return View(cat_data);

        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _uow.Category.GetAll().Select(u => new SelectListItem
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
                productVM.Product= _uow.Product.Get(u=>u.Id==id);
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
                    List<ShoppingCart> shoppingCarts = _uow.ShoppingCart.GetAll().ToList();
                    foreach (var cart in shoppingCarts)
                    {
                        if (cart.ProductId == obj.Product.Id)
                        {
                            var user = _uow.ApplicationUser.Get(u => u.Id == cart.ApplicationUserId);
                            var email = user.Email;
                            var subject = "Discount Alert";
                            var body = "Hey!There is a discount in the product that is in your cart.check it out";

                            await _emailSender.SendEmailAsync(email, subject, body);

                            cart.LastEmail = DateTime.Now;
                            _uow.ShoppingCart.Update(cart);
                            _uow.Save();
                        }
                    }
                }

                if (obj.Product.Id == 0)
                {
                    _uow.Product.Add(obj.Product);
                    TempData["success"] = "Product Created Successfully. ";

                }
                else
                {
                    _uow.Product.Update(obj.Product);
                    TempData["success"] = "Product updated Successfully. ";

                }
                _uow.Save();
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
            var data = _uow.Product.Get(u => u.Id == id);
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
            _uow.Product.Remove(obj);
            _uow.Save();
            TempData["delete"] = "Product deleted successfully. ";
            return RedirectToAction("Index");

        }
    }
}
