using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Application.ViewModel;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class ProductsService : IProductsService
    {
        private readonly IUnitOfWork uow;
        private readonly IAppEmailSender emailSender;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductsService(IUnitOfWork uow, IAppEmailSender emailSender, IWebHostEnvironment webHostEnvironment)
        {
            this.uow = uow;
            this.emailSender = emailSender;
            this.webHostEnvironment = webHostEnvironment;
        }


        public void AddProduct(Product product)
        {
            uow.Product.Add(product);
            uow.Save();
        }

        public void DeleteProduct(Product product)
        {
            uow.Product.Remove(product);
            uow.Save();
        }

        public IEnumerable<Product> GetAllProducts()
        {
            var data = uow.Product.GetAll(includeProperties: "Category");
            return data;
        }

        public IEnumerable<Product> GetAllSimilar(List<int> categoryIds, List<int> excludeProductIds)
        {
            var data = uow.Product.GetAll(p => categoryIds.Contains(p.CategoryId) && !excludeProductIds.Contains(p.Id),includeProperties: "Category").ToList();
            return data;
        }

        public Product GetProductById(int id)
        {
            var data = uow.Product.Get(u => u.Id == id, includeProperties: "Category");
            return data;
        }

        public void InsertImage(ProductVM obj, IFormFile? file)
        {
            string wwwRootPath = webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.Product.ImageUrl = @"\images\product\" + fileName;
            }
        }

        public async Task SendEmailToUserAsync(ShoppingCart cart)
        {
            var user = uow.ApplicationUser.Get(u => u.Id == cart.ApplicationUserId);
            var email = user.Email;
            var subject = "Discount Alert";
            var body = $"Hey!There is a discount in the product that is in your cart.check it out. The {cart.Product.ProductName} has {cart.Product.DiscountPercent}% off in it";
            await emailSender.SendEmailAsync(email, subject, body);
        }

        public void UpdateProduct(Product product)
        {
            uow.Product.Update(product);
            uow.Save();
        }

        
    }
}
