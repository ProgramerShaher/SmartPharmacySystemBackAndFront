using SmartPharmacySystem.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IUserShiftRepository
{
    Task<UserShift> GetByIdAsync(int id);
    Task<IEnumerable<UserShift>> GetAllAsync();
    Task AddAsync(UserShift entity);
    Task UpdateAsync(UserShift entity);
    Task DeleteAsync(int id);
}
