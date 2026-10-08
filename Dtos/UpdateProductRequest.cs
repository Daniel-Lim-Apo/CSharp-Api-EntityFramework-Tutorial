using System.ComponentModel.DataAnnotations;

namespace LojaApi.Dtos;

public sealed class UpdateProductRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0,01", "999999,99")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }
}
