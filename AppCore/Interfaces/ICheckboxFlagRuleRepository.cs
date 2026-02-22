using AppCore.EntityModels;
using AppCore.Helper;

namespace AppCore.Interfaces
{
    public interface ICheckboxFlagRuleRepository
    {
        Task<PaginatedList<CheckboxFlagRule>> SearchAsync(string column, string search, int pageIndex = 1, int pageSize = 25);
        Task<CheckboxFlagRule?> GetAsync(string skey);
        Task<CheckboxFlagRule?> GetAsync(int skey);
        Task<CheckboxFlagRule> AddAsync(CheckboxFlagRule model);
        Task<CheckboxFlagRule> UpdateAsync(CheckboxFlagRule model);
        Task<CheckboxFlagRule> DeleteAsync(CheckboxFlagRule model);
    }
}
