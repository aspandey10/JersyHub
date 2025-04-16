using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JersyHub.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Required]
       
        public int QuantityInStock { get; set; }

       
        public int QuantitySold { get; set; } = 0;

        [Required]
        public string Distributer { get; set; }
        public DateTime LastEmail { get; set; }



    }
}
