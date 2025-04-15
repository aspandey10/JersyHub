using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceImplementation;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Application.ViewModel;
using JersyHub.Application.ViewModel;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace JersyHub.Areas.Admin.Controllers
{
    [Area("Admin")]  
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IAppEmailSender _emailsender;
        private readonly IOrderHeaderService _orderheaderservice;
        private readonly IShoppingCartService _shoppingcartservice;
        private readonly IInventoryService _inventoryservice;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController( IAppEmailSender emailsender, IOrderHeaderService orderheaderservice, IShoppingCartService shoppingcartservice, IInventoryService inventoryservice)
        {
            _emailsender = emailsender;
            _orderheaderservice = orderheaderservice;
            _shoppingcartservice = shoppingcartservice;
            _inventoryservice = inventoryservice;
        }
        public IActionResult Index()
        {
            var orders = _orderheaderservice.GetOrders(User);
            return View(orders);

        }

        public IActionResult Details(int id)
        {
            var orderVM= _orderheaderservice.GetDetails(id);
            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            int orderId = OrderVM.OrderHeader.Id;
            var orderHeaderFromDb = _orderheaderservice.GetOrderHeader(orderId);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.NearestLandmark = OrderVM.OrderHeader.NearestLandmark;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _orderheaderservice.UpdateOrderHeader(orderHeaderFromDb);
            return RedirectToAction(nameof(Details), new { id = orderHeaderFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult StartProcessing()
        {
            int orderId = OrderVM.OrderHeader.Id;
            var status = StaticDetail.StatusInProcess;

            _orderheaderservice.UpdateStatus(orderId, status);
            return RedirectToAction(nameof(Details), new { Id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult ShipOrder()
        {
            int orderId= OrderVM.OrderHeader.Id;
            var orderHeader = _orderheaderservice.GetOrderHeader(orderId);
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = DateTime.Now;
            orderHeader.OrderStatus = StaticDetail.StatusShipped;
            _orderheaderservice.UpdateOrderHeader(orderHeader);


            _orderheaderservice.UpdateStatus(OrderVM.OrderHeader.Id, StaticDetail.StatusShipped);
            return RedirectToAction(nameof(Details), new { Id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _orderheaderservice.GetOrderHeader(OrderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == StaticDetail.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _orderheaderservice.UpdateStatus(orderHeader.Id, StaticDetail.StatusCancelled, StaticDetail.StatusRefunded);

            }
            else
            {
                _orderheaderservice.UpdateStatus(orderHeader.Id, StaticDetail.StatusCancelled, StaticDetail.StatusCancelled);
            }

            return RedirectToAction(nameof(Details), new { Id = OrderVM.OrderHeader.Id });


        }
    }
}
