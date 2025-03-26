using JersyHub.Application.Repository.IRepository;
using JersyHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Services
{
    public class EmailRemainderService
    {
        public IUnitOfWork _uow { get; }
        public IAppEmailSender _emailSender { get; }
        public EmailRemainderService(IUnitOfWork uow, IAppEmailSender emailSender)
        {
            _uow = uow;
            _emailSender = emailSender;
        }

        public async Task SendReminderEmails()
        {
            List<ShoppingCart> shoppingCarts = _uow.ShoppingCart.GetAll().ToList();

            foreach (var cartItem in shoppingCarts)
            {
                if (cartItem.AddedDate.AddDays(3) <= DateTime.Now) // Check if added 3+ days ago
                {
                    if (cartItem.LastEmail == DateTime.MinValue || cartItem.LastEmail.AddDays(3) <= DateTime.Now) // Check if 3 days passed since last email
                    {
                        var user = _uow.ApplicationUser.Get(u => u.Id == cartItem.ApplicationUserId);
                        var email = user.Email;
                        var subject = "Order Pending";
                        var body = "Hey! You still have items in your cart. Let's shop.";

                        await _emailSender.SendEmailAsync(email, subject, body);

                        cartItem.LastEmail = DateTime.Now; // Update LastEmail date
                        _uow.ShoppingCart.Update(cartItem);
                        _uow.Save();
                    }
                }
            }
        }
    }
}
