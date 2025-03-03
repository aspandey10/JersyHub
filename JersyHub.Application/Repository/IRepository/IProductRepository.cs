using JersyHub.Models;

namespace JersyHub.Application.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
    }
}
