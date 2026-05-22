using System.ComponentModel.DataAnnotations;
using Aras.Domain;

namespace Aras.Contracts;

public sealed class OrderEditRequest
{
    [Required]
    public OrderSide? Side { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
