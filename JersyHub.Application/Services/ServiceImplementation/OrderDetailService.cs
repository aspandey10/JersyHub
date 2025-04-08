using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class OrderDetailService: IOrderDetailService
    {
        private readonly IUnitOfWork uow;
        public OrderDetailService(IUnitOfWork uow)
        {
            this.uow = uow;
        }
        public void AddOrderDetail(OrderDetail orderDetail)
        {
            uow.OrderDetail.Add(orderDetail);
            uow.Save();
        }
    }

}
