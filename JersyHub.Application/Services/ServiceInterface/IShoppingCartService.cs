using JersyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface IShoppingCartService
    {
        void AddToCart(ShoppingCart shoppingCart);
        void RemoveFromCart(int cartId);
        void UpdateCart(ShoppingCart shoppingCart);
        ShoppingCart GetCartById(int cartId);
        IEnumerable<ShoppingCart> GetAllCarts(int?userid=null);
        IEnumerable<ShoppingCart> GetCartsByUserId(string userId);
        decimal GetTotalPrice(string userId);
    }
}
