using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JersyHub.Application.ViewModel;
using JersyHub.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace JersyHub.Application.Services.ServiceInterface
{
    public interface IOrderHeaderService
    {
        List<OrderVM> GetOrders(ClaimsPrincipal User);
        OrderVM GetDetails(int id);

        OrderHeader GetOrderHeader(int id);
        void UpdateOrderHeader(OrderHeader orderHeader);
        void UpdateStatus(int id, string  status, string? paymentStatus = null);
        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
        void AddOrderHeader(OrderHeader orderHeader);
    }
}
