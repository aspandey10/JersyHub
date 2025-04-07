  using JersyHub.Domain.Entities;
using JersyHub.Models;

namespace JersyHub.Application.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<Product> SimilarProducts { get; set; }

    }
}
