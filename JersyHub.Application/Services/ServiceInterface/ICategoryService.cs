using JersyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        public Category GetCategoryById(int id);
        void CreateCategory(Category category);
        void UpdateCategory( Category category);
        void DeleteCategory(Category category);
    }
}
