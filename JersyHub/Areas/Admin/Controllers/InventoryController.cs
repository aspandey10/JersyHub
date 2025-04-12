using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceImplementation;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Application.ViewModel;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JersyHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class InventoryController : Controller
    {
        
        private readonly IInventoryService _inventoryservice;
        private readonly IProductsService _productservice;

        public InventoryController( IInventoryService inventoryservice, IProductsService productservice)
        {

            _inventoryservice = inventoryservice;
            _productservice = productservice;
        } 


        public IActionResult Index()
        {
            var cat_data = _inventoryservice.GetAllInventories();
            return View(cat_data);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> ProductList = _productservice.GetAllProducts().Select(u => new SelectListItem
            {
                Text = u.ProductName,
                Value = u.Id.ToString()
            });
            InventoryVM inventoryVM = new()
            {
                ProductList = ProductList,
                Inventory = new Inventory()
            };
            return View(inventoryVM);
          
        }
        [HttpPost]
        public IActionResult Create(InventoryVM obj)
        {
            if (ModelState.IsValid)
            {
                _inventoryservice.CreateInventory(obj.Inventory);
                TempData["success"] = "Inventory Created Successfully. ";
                return RedirectToAction("Index");
            }
            return View();

        }
        public IActionResult Edit(int? id)
        {
            IEnumerable<SelectListItem> ProductList = _productservice.GetAllProducts().Select(u => new SelectListItem
            {
                Text = u.ProductName,
                Value = u.Id.ToString()
            });
            InventoryVM inventoryVM = new()
            {
                ProductList = ProductList,
                Inventory = new Inventory()
            };
            inventoryVM.Inventory = _inventoryservice.GetInventoryById((int)id);
            return View(inventoryVM);

        }
        [HttpPost]
        public IActionResult Edit(InventoryVM obj)
        {
            if (ModelState.IsValid)
            {
                _inventoryservice.UpdateInventory(obj.Inventory);
                TempData["success"] = "Inventory Updated Successfully. ";
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
            var data = _inventoryservice.GetInventoryById(id.Value);
            if (data == null)
            {
                return NotFound();
            }

            return View(data);
        }
        [HttpPost]
        public IActionResult Delete(Inventory obj)
        {
            if (obj == null)
            {
                return NotFound();
            }
            _inventoryservice.DeleteInventory(obj);
            TempData["delete"] = "Inventory deleted successfully. ";
            return RedirectToAction("Index");

        }



    }
}
