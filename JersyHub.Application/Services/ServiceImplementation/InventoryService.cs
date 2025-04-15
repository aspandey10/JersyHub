using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork uow;
        public InventoryService(IUnitOfWork uow)
        {
            this.uow = uow;
        }
        public void CreateInventory(Inventory inventory)
        {
            uow.Inventory.Add(inventory);
            uow.Save();
        }

        public void DeleteInventory(Inventory inventory)
        {
            uow.Inventory.Remove(inventory);
            uow.Save();
        }

        public IEnumerable<Inventory> GetAllInventories()
        {
            var inventories = uow.Inventory.GetAll(includeProperties: "Product");
            return inventories;
        }
        public Inventory GetInventoryById(int id)
        {
            var data = uow.Inventory.Get(u => u.Id == id, includeProperties: "Product");
            return data;
        }

        public Inventory GetInventoryByProductId(int productId)
        {
            var data = uow.Inventory.Get(u => u.ProductId == productId, includeProperties: "Product");
            return data;
        }

        public void UpdateInventory(Inventory inventory)
        {
            uow.Inventory.Update(inventory);
            uow.Save();
        }

       
    }
    
}
