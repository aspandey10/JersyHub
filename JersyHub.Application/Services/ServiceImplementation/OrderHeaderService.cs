using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class OrderHeaderService : IOrderHeaderService
    {
        private readonly IUnitOfWork uow;

        public OrderHeaderService(IUnitOfWork uow)
        {
            this.uow = uow;
        }
        public List<OrderVM> GetOrders(ClaimsPrincipal User)
        {
            if (User.IsInRole(StaticDetail.Role_Admin) || User.IsInRole(StaticDetail.Role_Employee))
            {
                List<OrderHeader> orderHeaders = uow.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

                List<OrderVM> orderVMs = orderHeaders.Select(order => new OrderVM
                {
                    OrderHeader = order,
                    OrderDetail = uow.OrderDetail.GetAll(u => u.OrderHeaderId == order.Id).ToList()
                }).ToList();
                return orderVMs;

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                List<OrderHeader> orderHeaders = uow.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();

                List<OrderVM> orderVMs = orderHeaders.Select(order => new OrderVM
                {
                    OrderHeader = order,
                    OrderDetail = uow.OrderDetail.GetAll(u => u.OrderHeaderId == order.Id).ToList()
                }).ToList();
                return orderVMs;

            }
        }

        public OrderVM GetDetails(int id)
        {
            var orderVM = new OrderVM()
            {
                OrderHeader = uow.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetail = uow.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product")
            };

            return orderVM;
        }

        public OrderHeader GetOrderHeader(int id)
        {
            var data=uow.OrderHeader.Get(u => u.Id == id);
            return data;
        }

        public void UpdateOrderHeader(OrderHeader orderHeader)
        {
            uow.OrderHeader.Update(orderHeader);
            uow.Save();
        }

        public void UpdateStatus(int id, string status, string? paymentStatus = null)
        {
            uow.OrderHeader.UpdateStatus(id, status,paymentStatus);
            uow.Save();
        }
    }
}
