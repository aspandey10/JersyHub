 using JersyHub.Domain.Entities;

namespace JersyHub.Models.ViewModel
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail>  OrderDetail { get; set; } 
    }
}
