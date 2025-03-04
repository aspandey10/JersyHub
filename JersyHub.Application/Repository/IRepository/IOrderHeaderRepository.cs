using JersyHub.Domain.Entities;
using JersyHub.Models;

namespace JersyHub.Application.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
    }
}
