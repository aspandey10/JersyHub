using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using JersyHub.Models.ViewModel;
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
        private readonly IUnitOfWork _uow;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public IActionResult Index()
        {

            if (User.IsInRole(StaticDetail.Role_Admin) || User.IsInRole(StaticDetail.Role_Employee))
            {
                List<OrderHeader> orderHeaders = _uow.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

                List<OrderVM> orderVMs = orderHeaders.Select(order => new OrderVM
                {
                    OrderHeader = order,
                    OrderDetail = _uow.OrderDetail.GetAll(u => u.OrderHeaderId == order.Id).ToList()
                }).ToList();
                return View(orderVMs);

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                List<OrderHeader> orderHeaders = _uow.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();

                List<OrderVM> orderVMs = orderHeaders.Select(order => new OrderVM
                {
                    OrderHeader = order,
                    OrderDetail = _uow.OrderDetail.GetAll(u => u.OrderHeaderId == order.Id).ToList()
                }).ToList();
                return View(orderVMs);

            }

        }







        public IActionResult Details(int id)
        {
            OrderVM = new()
            {
                OrderHeader = _uow.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetail = _uow.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _uow.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
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
            _uow.OrderHeader.Update(orderHeaderFromDb);
            _uow.Save();
            return RedirectToAction(nameof(Details), new { id = orderHeaderFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _uow.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, StaticDetail.StatusInProcess);
            _uow.Save();
            return RedirectToAction(nameof(Details), new { Id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _uow.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = DateTime.Now;
            orderHeader.OrderStatus = StaticDetail.StatusShipped;
            _uow.OrderHeader.Update(orderHeader);


            _uow.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, StaticDetail.StatusShipped);
            _uow.Save();
            return RedirectToAction(nameof(Details), new { Id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _uow.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
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

                _uow.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.StatusCancelled, StaticDetail.StatusRefunded);

            }
            else
            {
                _uow.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.StatusCancelled, StaticDetail.StatusCancelled);
            }

            _uow.Save();
            return RedirectToAction(nameof(Details), new { Id = OrderVM.OrderHeader.Id });


        }
    }
}
