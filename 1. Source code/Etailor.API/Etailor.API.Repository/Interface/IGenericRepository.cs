using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Interface
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Func<T, bool> where);
        IEnumerable<T> GetAllPagination(Func<T, bool> where, int? pageIndex, int? itemPerPage);
        int Count(Func<T, bool> where);
        T FirstOrDefault(Func<T, bool> where);
        T Get(string id);
        Task<T> GetAsync(string id);
        bool Create(T entity);
        Task<bool> CreateAsync(T entity);
        bool CreateRange(List<T> entities);
        Task<bool> CreateRangeAsync(List<T> entities);
        bool Update(string id, T entity);
        Task<bool> UpdateAsync(string id, T entity);
        bool UpdateRange(List<T> entities);
        Task<bool> UpdateRangeAsync(List<T> entities);
        bool Delete(string id);
        Task<bool> DeleteAsync(string id);
        void Detach(string id);
        void SaveChange();
        Task SaveChangeAsync();
        IEnumerable<T> GetStoreProcedure(string storeProcedure,params SqlParameter[]? parameters);
    }
}
