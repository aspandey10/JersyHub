using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using System.Linq.Expressions;

namespace JersyHub.Infrastructure.Repo
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {    
        private readonly ApplicationDbContext _db;


        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj); 
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u=>u.Id==id);
            if (orderFromDb != null) {
                orderFromDb.OrderStatus = orderStatus;
                if(paymentStatus != null)
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u=>u.Id==id);
            if (sessionId!=null)
            {
                orderFromDb.SessionId = sessionId;
            }
            if (paymentIntentId != null)
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate=DateTime.Now;
            }
        }
    }
}
