using Microsoft.EntityFrameworkCore;
using TacosApp.Admin.Data;
using TacosApp.Admin.Models;

namespace TacosApp.Admin.Services;

public sealed class MenuService(AdminDbContext db) : IMenuService
{
    public async Task<IReadOnlyList<Menu>> GetAllAsync(CancellationToken ct = default) =>
        await db.Menus
            .AsNoTracking()
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);

    public Task<Menu?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Menus.FirstOrDefaultAsync(m => m.MenuId == id, ct);

    public async Task<Menu> CreateAsync(Menu menu, CancellationToken ct = default)
    {
        db.Menus.Add(menu);
        await db.SaveChangesAsync(ct);
        return menu;
    }

    public async Task<bool> UpdateAsync(Menu menu, CancellationToken ct = default)
    {
        var existing = await db.Menus.FirstOrDefaultAsync(m => m.MenuId == menu.MenuId, ct);
        if (existing is null) return false;

        existing.Name = menu.Name;
        existing.Description = menu.Description;
        existing.Price = menu.Price;
        existing.ImageUrl = menu.ImageUrl;
        existing.IsAvailable = menu.IsAvailable;
        existing.DisplayOrder = menu.DisplayOrder;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var rows = await db.Menus.Where(m => m.MenuId == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }
}
