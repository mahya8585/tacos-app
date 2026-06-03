using Microsoft.EntityFrameworkCore;
using TacosApp.Admin.Data;
using TacosApp.Admin.Models;

namespace TacosApp.Admin.Services;

public sealed class ToppingService(AdminDbContext db) : IToppingService
{
    public async Task<IReadOnlyList<Topping>> GetAllAsync(CancellationToken ct = default) =>
        await db.Toppings
            .AsNoTracking()
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);

    public Task<Topping?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Toppings.FirstOrDefaultAsync(t => t.ToppingId == id, ct);

    public async Task<Topping> CreateAsync(Topping topping, CancellationToken ct = default)
    {
        db.Toppings.Add(topping);
        await db.SaveChangesAsync(ct);
        return topping;
    }

    public async Task<bool> UpdateAsync(Topping topping, CancellationToken ct = default)
    {
        var existing = await db.Toppings.FirstOrDefaultAsync(t => t.ToppingId == topping.ToppingId, ct);
        if (existing is null) return false;

        existing.Name = topping.Name;
        existing.Price = topping.Price;
        existing.IsAvailable = topping.IsAvailable;
        existing.DisplayOrder = topping.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var rows = await db.Toppings.Where(t => t.ToppingId == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }
}
