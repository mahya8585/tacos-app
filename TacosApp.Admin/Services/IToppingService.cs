using TacosApp.Admin.Models;

namespace TacosApp.Admin.Services;

public interface IToppingService
{
    Task<IReadOnlyList<Topping>> GetAllAsync(CancellationToken ct = default);
    Task<Topping?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Topping> CreateAsync(Topping topping, CancellationToken ct = default);
    Task<bool> UpdateAsync(Topping topping, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
