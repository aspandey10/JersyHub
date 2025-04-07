using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Models;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class ProductsService : IProductsService
    {
        private readonly IUnitOfWork uow;
        private readonly IEmailSender emailSender;

        public ProductsService(IUnitOfWork uow, IEmailSender emailSender )
        {
            this.uow = uow;
            this.emailSender = emailSender;
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

        public Product GetProductById(int id)
        {
            var data = uow.Product.Get(u => u.Id == id);
            return data;
        }

        public async Task SendEmailToUserAsync(ClaimsPrincipal User)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = uow.ApplicationUser.Get(u => u.Id == userId);
            var email = user.Email;
            var subject = "Discount Alert";
            var body = "Hey!There is a discount in the product that is in your cart.check it out";
            await emailSender.SendEmailAsync(email, subject, body);
        }

        public void UpdateProduct(Product product)
        {
            uow.Product.Update(product);
            uow.Save();
        }

        
    }
}
