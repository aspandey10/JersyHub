using JersyHub.Domain.Entities;
using JersyHub.Models;

namespace JersyHub.Application.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj);
    }
}
