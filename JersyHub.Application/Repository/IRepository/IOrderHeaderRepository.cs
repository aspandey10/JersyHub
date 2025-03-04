using JersyHub.Domain.Entities;
using JersyHub.Models;

namespace JersyHub.Application.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string orderStatus,string? paymentStatus=null);
        void UpdateStripePaymentId(int id, string sessionId,string paymentIntentId);
    }
}
