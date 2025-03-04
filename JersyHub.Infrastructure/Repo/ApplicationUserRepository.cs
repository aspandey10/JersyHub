using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using System.Linq.Expressions;

namespace JersyHub.Infrastructure.Repo
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;


        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
      
    }
}
