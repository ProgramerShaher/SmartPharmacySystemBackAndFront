using System.Linq.Expressions;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate);
    Task<Permission?> GetByCodeAsync(string code);
}
