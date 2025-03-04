using JersyHub.Domain.Entities;
using JersyHub.Models;

namespace JersyHub.Application.Repository.IRepository
{
    public interface IOrderDeatilRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
    }
}
