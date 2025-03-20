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
        private readonly IAppEmailSender _emailsender;

        [BindProperty] 
        public ShoppingCartVM shoppingCartVM { get; set; }

        public CartController(IUnitOfWork uow, IAppEmailSender emailsender) 
        {
            _uow = uow;
            _emailsender = emailsender;
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product,Product.Category"),
                OrderHeader = new()
            }; 
            
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            var categoryIds = shoppingCartVM.ShoppingCartList.Select(c => c.Product.CategoryId).Distinct().ToList();

            var similarProducts = _uow.Product.GetAll(p => categoryIds.Contains(p.CategoryId) &&!shoppingCartVM.ShoppingCartList.Select(c => c.ProductId).Contains(p.Id),includeProperties: "Category")
            .ToList();

            shoppingCartVM.SimilarProducts = similarProducts;

            return View(shoppingCartVM);
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
                HttpContext.Session.SetInt32(StaticDetail.SessionCart, _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == cartDb.ApplicationUserId).Count() - 1);
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
            HttpContext.Session.SetInt32(StaticDetail.SessionCart, _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == cartDb.ApplicationUserId).Count() - 1);
            _uow.ShoppingCart.Remove(cartDb);
            _uow.Save();
            return RedirectToAction("Index");
        }

        public IActionResult CheckOut()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = _uow.ApplicationUser.Get(u => u.Id == userId);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;  
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.Address;
            shoppingCartVM.OrderHeader.PhoneNumber= shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber; 


            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;

                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("CheckOut")]
        public IActionResult CheckOutPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.ShoppingCartList = _uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;


            shoppingCartVM.OrderHeader.ApplicationUser = _uow.ApplicationUser.Get(u => u.Id == userId);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.Address;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;



            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.Price;

                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            shoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusPending;
            shoppingCartVM.OrderHeader.OrderStatus = StaticDetail.StatusPending;

            _uow.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _uow.Save();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _uow.OrderDetail.Add(orderDetail);
                _uow.Save();
            }
            var domain = "https://localhost:7120/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {

                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/Index",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),

                Mode = "payment",
            };

            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                {
                    PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price * 100,
                        Currency = "inr",
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
            _uow.OrderHeader.UpdateStripePaymentId(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _uow.Save();
            Response.Headers.Add("Location",session.Url);
            return new StatusCodeResult(303);



        }


        public async Task<IActionResult> OrderConfirmationAsync(int id)
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

            // Send Order Confirmation Email
            string customerEmail = orderHeader.ApplicationUser.Email;
            string subject = "Order Confirmation - JersyHub";
            string body = $"<h2>Thank you for your order!</h2><p>Your order #{id} has been successfully placed.</p>";

            await _emailsender.SendEmailAsync(customerEmail, subject, body);

            return View(id);
        }


    }
}
