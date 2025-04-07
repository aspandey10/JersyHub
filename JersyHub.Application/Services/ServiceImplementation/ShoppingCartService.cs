using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IUnitOfWork uow;

        public ShoppingCartService(IUnitOfWork uow)
        {
            this.uow = uow;
        }


        public void AddToCart(ShoppingCart shoppingCart)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShoppingCart> GetAllCarts()
        {
            var data = uow.ShoppingCart.GetAll();
            return data;
        }

        public ShoppingCart GetCartById(int cartId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShoppingCart> GetCartsByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public decimal GetTotalPrice(string userId)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromCart(int cartId)
        {
            throw new NotImplementedException();
        }

        public void UpdateCart(ShoppingCart shoppingCart)
        {
            throw new NotImplementedException();
        }
    }
}
