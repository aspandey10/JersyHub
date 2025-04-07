 using JersyHub.Domain.Entities;

namespace JersyHub.Application.ViewModel
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail>  OrderDetail { get; set; } 
    }
}
