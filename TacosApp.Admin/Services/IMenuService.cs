using TacosApp.Admin.Models;

namespace TacosApp.Admin.Services;

public interface IMenuService
{
    Task<IReadOnlyList<Menu>> GetAllAsync(CancellationToken ct = default);
    Task<Menu?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Menu> CreateAsync(Menu menu, CancellationToken ct = default);
    Task<bool> UpdateAsync(Menu menu, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
