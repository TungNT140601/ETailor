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
        bool Create(T entity);
        bool CreateRange(List<T> entities);
        bool Update(string id, T entity);
        bool Delete(string id);
        void Detach(string id);
    }
}
