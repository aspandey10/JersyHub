﻿  using JersyHub.Domain.Entities;

namespace JersyHub.Models.ViewModel
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<Product> SimilarProducts { get; set; }

    }
}
