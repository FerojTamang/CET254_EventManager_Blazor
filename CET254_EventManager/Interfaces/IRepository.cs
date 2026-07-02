using CET254_EventManager.Models;

namespace CET254_EventManager.Interfaces
{
    // Repository contract used by services instead of depending on EF Core directly.
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);

        Task<List<T>> GetAllAsync();

        IQueryable<T> Query();

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<bool> ExistsAsync(int id);

        Task<int> CountAsync();

        Task SaveChangesAsync();
    }
}
