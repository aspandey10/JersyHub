using JersyHub.Application.Repository.IRepository;
using JersyHub.Application.Services.ServiceInterface;
using JersyHub.Application.ViewModel;
using JersyHub.Data;
using JersyHub.Domain.Entities;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services.ServiceImplementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IUnitOfWork uow;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IOrderHeaderService _orderHeaderService;
        private readonly IInventoryService _inventoryservice;
        private readonly string _domain = "https://localhost:7120/";

        public ShoppingCartService(IUnitOfWork uow, IOrderDetailService orderDetailService, IOrderHeaderService orderHeaderService,IInventoryService inventoryservice)
        {
            this.uow = uow;
            _orderDetailService = orderDetailService;
            _orderHeaderService = orderHeaderService;
            _inventoryservice = inventoryservice;
        }


        public void AddToCart(ShoppingCart shoppingCart)
        {
            uow.ShoppingCart.Add(shoppingCart);
            uow.Save();
        }

        public IEnumerable<ShoppingCart> GetAllCarts()
        {
            var data = uow.ShoppingCart.GetAll();
            return data;
        }

        public IEnumerable<ShoppingCart> GetCartsForUser(string userId)
        {
            var data = uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product,Product.Category");
            return data;
        }

        public ShoppingCart GetCartById(int cartId)
        {
            var data = uow.ShoppingCart.Get(u => u.Id == cartId );
            return data;
        }

        

        public void RemoveFromCart(ShoppingCart shoppingcart)
        {
            uow.ShoppingCart.Remove(shoppingcart);
            uow.Save();
        }

        public void UpdateCart(ShoppingCart shoppingCart)
        {
            uow.ShoppingCart.Update(shoppingCart);
            uow.Save();
        }

        public void ClearCart(List<ShoppingCart> shoppingCarts)
        {
            uow.ShoppingCart.RemoveRange(shoppingCarts);
            uow.Save();
        }

        public Stripe.Checkout.Session CreateCheckoutSession(ShoppingCartVM shoppingCartVM, string userId)
        {
            shoppingCartVM.ShoppingCartList = uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product,Product.Category");
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

            shoppingCartVM.OrderHeader.ApplicationUser = uow.ApplicationUser.Get(u => u.Id == userId);
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = cart.Product.DiscountPercent > 0 ? cart.Product.DiscountedPrice : cart.Product.Price;
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            shoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusPending;
            shoppingCartVM.OrderHeader.OrderStatus = StaticDetail.StatusPending;

            _orderHeaderService.AddOrderHeader(shoppingCartVM.OrderHeader);

            // Add order details for each product in the shopping cart
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _orderDetailService.AddOrderDetail(orderDetail);
            }

            // Create Stripe session
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = _domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = _domain + "customer/cart/Index",
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
            return service.Create(options);
        }

        public ShoppingCartVM Checkout(ClaimsPrincipal User)
        {
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = uow.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product,Product.Category"),
                OrderHeader = new()
            };
           

            shoppingCartVM.OrderHeader.ApplicationUser = uow.ApplicationUser.Get(ApplicationUser => ApplicationUser.Id == userId);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.Address;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;


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
            return shoppingCartVM;
        }

        public ShoppingCart GetCart(string userId, int productId)
        {
            var data=uow.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == productId);
            return data;
        }

        
    }
}

