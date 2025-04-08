using JersyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface IOrderDetailService
    {
        void AddOrderDetail(OrderDetail orderDetail);
    }
}
