using JersyHub.Application.ViewModel;
using JersyHub.Domain.Entities;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface IShoppingCartService
    {
        void AddToCart(ShoppingCart shoppingCart);
        void RemoveFromCart(ShoppingCart shoppingCart);
        void ClearCart(List<ShoppingCart> shoppingCarts);
        void UpdateCart(ShoppingCart shoppingCart);
        ShoppingCart GetCartById(int cartId);
        ShoppingCart GetCart(string userId, int productId);
        IEnumerable<ShoppingCart> GetAllCarts();
        IEnumerable<ShoppingCart> GetCartsForUser(string userId);
        Stripe.Checkout.Session CreateCheckoutSession(ShoppingCartVM shoppingCartVM, string userId);
        ShoppingCartVM Checkout(ClaimsPrincipal User);
    }
}
