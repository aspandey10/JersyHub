using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Models;
using System.Linq.Expressions;

namespace JersyHub.Infrastructure.Repo
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ApplicationDbContext Db { get; }

        public ProductRepository(ApplicationDbContext db):base(db)
        {
            Db = db;
        }


        public void Update(Product obj)
        {
            Db.Products.Update(obj);
        }
    }
}
