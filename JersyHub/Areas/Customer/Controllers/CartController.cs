using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceImplementation;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Application.ViewModel;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using JersyHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Issuing;
using System.Security.Claims;

namespace JersyHub.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    { 
        
        private readonly IAppEmailSender _emailsender;
        private readonly IProductsService _productservice;
        private readonly IShoppingCartService _shoppingcartservice;
        private readonly IOrderHeaderService _orderheaderservice;
        private readonly IOrderDetailService _orderdetailservice;
        private readonly IInventoryService _inventoryservice;

        [BindProperty] 
        public ShoppingCartVM shoppingCartVM { get; set; }

        public CartController(IAppEmailSender emailsender, IProductsService productservice, IShoppingCartService shoppingcartservice,IOrderHeaderService orderheaderservice, IOrderDetailService orderdetailservice, IInventoryService inventoryservice)
        {

            _emailsender = emailsender;
            _productservice = productservice;
            _shoppingcartservice = shoppingcartservice;
            _orderheaderservice = orderheaderservice;
            _orderdetailservice = orderdetailservice;
            _inventoryservice = inventoryservice;
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList =_shoppingcartservice.GetCartsForUser(userId),
                OrderHeader = new()
            }; 
            
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                if (cart.Product.DiscountPercent > 0)
                {
                    cart.Price = cart.Product.DiscountedPrice;

                }
                else
                {
                    cart.Price = cart.Product.Price;

                }
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            var categoryIds = shoppingCartVM.ShoppingCartList.Select(c => c.Product.CategoryId).Distinct().ToList();
            var cartProductIds = shoppingCartVM.ShoppingCartList.Select(c => c.ProductId).ToList();

            var similarProducts = _productservice.GetAllSimilar(categoryIds, cartProductIds);

            shoppingCartVM.SimilarProducts = similarProducts;

            return View(shoppingCartVM);
        }

        public IActionResult Plus(int cartId) 
        {
            var cartDb = _shoppingcartservice.GetCartById(cartId);
            cartDb.Count += 1;
            _shoppingcartservice.UpdateCart(cartDb);
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var cartDb = _shoppingcartservice.GetCartById(cartId);
            if (cartDb.Count <= 1)
            {
                HttpContext.Session.SetInt32(StaticDetail.SessionCart,_shoppingcartservice.GetCartsForUser(cartDb.ApplicationUserId).Count() - 1);
                _shoppingcartservice.RemoveFromCart(cartDb);
            }
            else
            {
                cartDb.Count -= 1;
                _shoppingcartservice.UpdateCart(cartDb);

            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cartDb =_shoppingcartservice.GetCartById(cartId);
            HttpContext.Session.SetInt32(StaticDetail.SessionCart, _shoppingcartservice.GetCartsForUser(cartDb.ApplicationUserId).Count() - 1);
            _shoppingcartservice.RemoveFromCart(cartDb);
            return RedirectToAction("Index");
        }

        public IActionResult CheckOut()
        {
            var shoppingCartVM= _shoppingcartservice.Checkout(User);
            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("CheckOut")]
        public IActionResult CheckOutPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;           
            var session = _shoppingcartservice.CreateCheckoutSession(shoppingCartVM, userId);
            _orderheaderservice.UpdateStripePaymentId(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }


        public async Task<IActionResult> OrderConfirmationAsync(int id)
        {
            OrderHeader orderHeader = _orderheaderservice.GetOrderHeader(id);
            var service = new SessionService();
            Session session= service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _orderheaderservice.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                _orderheaderservice.UpdateStatus(id, StaticDetail.StatusApproved, StaticDetail.PaymentStatusApproved);
                
            }

            List<ShoppingCart> shoppingCarts = _shoppingcartservice.GetCartsForUser(orderHeader.ApplicationUserId).ToList();
            foreach (var cart in shoppingCarts)
            {
                var invdata = _inventoryservice.GetInventoryByProductId(cart.ProductId);
                var count = cart.Count;
                invdata.QuantitySold += count;
                invdata.QuantityInStock -= count;
                _inventoryservice.UpdateInventory(invdata);
            }
            _shoppingcartservice.ClearCart(shoppingCarts);
            

            // Send Order Confirmation Email
            string customerEmail = orderHeader.ApplicationUser.Email;
            string subject = "Order Confirmation - JersyHub";
            string body = $"<h2>Thank you for your order!</h2><p>Your order #{id} has been successfully placed.</p>";          
            await _emailsender.SendEmailAsync(customerEmail, subject, body);
            if (HttpContext?.Session != null)
            {
                HttpContext.Session.Clear();
            }
            return RedirectToAction("Index", "Order", new {area="Admin"});
        }


    }
}
