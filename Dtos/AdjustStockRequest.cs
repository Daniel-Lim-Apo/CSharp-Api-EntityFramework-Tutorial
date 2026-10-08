using System.ComponentModel.DataAnnotations;

namespace LojaApi.Dtos;

public sealed class AdjustStockRequest
{
    [Range(-100000, 100000)]
    public int Quantity { get; init; }
}
