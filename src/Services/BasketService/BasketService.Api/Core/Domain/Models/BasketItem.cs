﻿using System.ComponentModel.DataAnnotations;

namespace BasketService.Api.Core.Domain.Models
{
    public class BasketItem : IValidatableObject
    {
        public string Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OldUnitPrice { get; set; }
        public int Quantity { get; set; }
        public string PictureUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = new List<ValidationResult>();

            if (Quantity < 1)
                result.Add(new ValidationResult("Invalid Number of units ", new[] { "Quantity" }));

            return result;
        }
    }
}
