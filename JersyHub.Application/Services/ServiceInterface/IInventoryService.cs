using JersyHub.Domain.Entities;
using JersyHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface IInventoryService
    {
        IEnumerable<Inventory> GetAllInventories();
        public Inventory GetInventoryById(int id);
        void CreateInventory(Inventory inventory);
        void UpdateInventory( Inventory inventory);
        void DeleteInventory(Inventory inventory);
        public Inventory GetInventoryByProductId(int productId);
    }
}
