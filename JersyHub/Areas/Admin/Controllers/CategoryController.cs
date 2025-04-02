using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JersyHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =StaticDetail.Role_Admin)]
    public class CategoryController : Controller
    {
        public IUnitOfWork _uow;

        public CategoryController(IUnitOfWork uow)
        {
            _uow = uow;
        } 


        public IActionResult Index()
        {
            var cat_data = _uow.Category.GetAll().ToList();
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
                _uow.Category.Add(obj);
                _uow.Save();
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
            var data = _uow.Category.Get(u => u.Id == id);
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
                _uow.Category.Update(obj);
                _uow.Save();
                TempData["edit"] = "Category edited successfully. ";
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
            var data = _uow.Category.Get(u => u.Id == id);
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
            _uow.Category.Remove(obj);
            _uow.Save();
            TempData["delete"] = "Category deleted successfully. ";
            return RedirectToAction("Index");

        }



    }
}
