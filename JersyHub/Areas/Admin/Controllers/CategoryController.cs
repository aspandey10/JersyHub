using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JersyHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
    {
        
        private readonly ICategoryService _categoryservice;

        public CategoryController( ICategoryService categoryservice)
        {
            
            _categoryservice = categoryservice;
        } 


        public IActionResult Index()
        {
            var cat_data = _categoryservice.GetAllCategories();
            return View(cat_data);
        }

        public IActionResult Create()
        {
            ViewBag.Countries = StaticDetail.Countries;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _categoryservice.CreateCategory(obj);
                TempData["success"] = "Category Created Successfully. ";
                return RedirectToAction("Index");
            }
            return View();

        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var data = _categoryservice.GetCategoryById(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _categoryservice.UpdateCategory(obj);
                TempData["success"] = "Category Updated Successfully. ";
                return RedirectToAction("Index");
            }
            return View();

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var data = _categoryservice.GetCategoryById(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }
        [HttpPost]
        public IActionResult Delete(Category obj)
        {
            if (obj == null)
            {
                return NotFound();
            }
            _categoryservice.DeleteCategory(obj);
            TempData["delete"] = "Category deleted successfully. ";
            return RedirectToAction("Index");

        }



    }
}
