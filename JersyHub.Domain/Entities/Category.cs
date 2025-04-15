using JersyHub.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JersyHub.Domain.Entities
{
    public class Category
    {   
        [Key]
        public int Id { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
        public string LeagueCountry { get; set; }
        [Range(1, 100, ErrorMessage = "The range must be between 0-100")]
        public int DisplayOrder { get; set; } = 0;
        [ValidateNever]
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }
 

    }
}
