namespace LojaApi.Models;

public sealed class StockMovement
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
