using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JersyHub.Application.Repository.IRepository
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
