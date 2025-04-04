using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork uow;

        public CategoryService(IUnitOfWork uow)
        {
            this.uow = uow;
        }
        public void CreateCategory(Category category)
        {
            uow.Category.Add(category);
            uow.Save();
        }

        public void DeleteCategory(Category category)
        {
            uow.Category.Remove(category);
            uow.Save();
        }

        public IEnumerable<Category> GetAllCategories()
        {
            var categories = uow.Category.GetAll();
            return categories;
        }

        public Category GetCategoryById(int id)
        {
            var data = uow.Category.Get(u => u.Id == id);
            return data;
        }

        public void UpdateCategory(Category category)
        {
            uow.Category.Update(category);
            uow.Save();
        }
    }
}
