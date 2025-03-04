using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using System.Linq.Expressions;

namespace JersyHub.Infrastructure.Repo
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDeatilRepository
    {
        private readonly ApplicationDbContext _db;


        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj); 
        }

      
    }
}
