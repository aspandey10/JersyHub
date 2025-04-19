using JersyHub.Application.ViewModel;
using JersyHub.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface IProductsService
    {
        void AddProduct(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int id);

        Task SendEmailToUserAsync(ClaimsPrincipal User);

        IEnumerable<Product> GetAllSimilar(List<int> catid, List<int> excludeProductId);
        void InsertImage(ProductVM obj, IFormFile? file);
    }
}
