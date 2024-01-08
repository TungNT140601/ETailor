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
        T Get(string id);
        Task<bool> Create(T entity);
        Task<bool> Update(string id, T entity);
        Task<bool> Delete(string id);
    }
}
