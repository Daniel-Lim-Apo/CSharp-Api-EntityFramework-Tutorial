using LojaApi.Data;
using LojaApi.Dtos;
using LojaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] decimal? minPrice,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        if (minPrice is not null)
            query = query.Where(p => p.Price >= minPrice);

        if (categoryId is not null)
            query = query.Where(p => p.CategoryId == categoryId);

        var result = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.CategoryId,
                p.Category.Name))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.CategoryId,
                p.Category.Name))
            .SingleOrDefaultAsync(cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return BadRequest(new ProblemDetails { Title = "Invalid category", Detail = "CategoryId does not exist." });

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        var response = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == product.Id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.Stock, p.CategoryId, p.Category.Name))
            .SingleAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
            return NotFound();

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return BadRequest(new ProblemDetails { Title = "Invalid category", Detail = "CategoryId does not exist." });

        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
            return NotFound();

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/stock")]
    public async Task<IActionResult> AdjustStock(
        int id,
        AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity == 0)
            return BadRequest(new ProblemDetails { Title = "Quantity must be different from zero." });

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (product is null)
                return NotFound();

            if (product.Stock + request.Quantity < 0)
                return BadRequest(new ProblemDetails { Title = "Insufficient stock." });

            product.Stock += request.Quantity;
            await db.SaveChangesAsync(cancellationToken);

            db.StockMovements.Add(new StockMovement
            {
                ProductId = id,
                Quantity = request.Quantity,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return NoContent();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
