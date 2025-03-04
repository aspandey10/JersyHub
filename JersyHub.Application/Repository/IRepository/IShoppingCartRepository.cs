using JersyHub.Domain.Entities;
using JersyHub.Models;

namespace JersyHub.Application.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart obj);
    }
}
