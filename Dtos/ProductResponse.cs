namespace LojaApi.Dtos;

public sealed record ProductResponse(
    int Id,
    string Name,
    decimal Price,
    int Stock,
    int CategoryId,
    string CategoryName);
