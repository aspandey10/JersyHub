using JersyHub.Application.Repository.IRepository;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

namespace JersyHub.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    { 
        private readonly IUnitOfWork _uow;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork uow)
        {
            _uow = uow;
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            }; 
            
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;

                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            } 

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartDb = _uow.ShoppingCart.Get(u => u.Id == cartId);
            cartDb.Count += 1;
            _uow.ShoppingCart.Update(cartDb);
            _uow.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var cartDb = _uow.ShoppingCart.Get(u => u.Id == cartId);
            if (cartDb.Count <= 1)
            {
                _uow.ShoppingCart.Remove(cartDb);
            }
            else
            {
                cartDb.Count -= 1;
                _uow.ShoppingCart.Update(cartDb);
                 
            }
            _uow.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cartDb = _uow.ShoppingCart.Get(u => u.Id == cartId);
            _uow.ShoppingCart.Remove(cartDb);
            _uow.Save();
            return RedirectToAction("Index");
        }

        public IActionResult CheckOut()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _uow.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;  
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.PhoneNumber= ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber; 


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;

                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("CheckOut")]
        public IActionResult CheckOutPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


            ShoppingCartVM.OrderHeader.ApplicationUser = _uow.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;



            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;

                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = StaticDetail.StatusPending;

            _uow.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _uow.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _uow.OrderDetail.Add(orderDetail);
                _uow.Save();
            }
            var domain = "https://localhost:7120/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {

                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/Index",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),

                Mode = "payment",
            };

            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price * 100,
                        Currency = "usd",
                        ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.ProductName,
                        }
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options); 
            _uow.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _uow.Save();
            Response.Headers.Add("Location",session.Url);
            return new StatusCodeResult(303);



        }


        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _uow.OrderHeader.Get(u => u.Id == id,includeProperties:"ApplicationUser");
            var service = new SessionService();
            Session session= service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _uow.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                _uow.OrderHeader.UpdateStatus(id, StaticDetail.StatusApproved, StaticDetail.PaymentStatusApproved);
                _uow.Save();
            }

            List<ShoppingCart> shoppingCarts = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _uow.ShoppingCart.RemoveRange(shoppingCarts);
            _uow.Save();

            return View(id);
        }


    }
}
