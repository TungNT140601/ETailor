using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.Interface;
using Etailor.API.Ultity.CustomException;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ETailor_DBContext dBContext;
        protected readonly DbSet<T> dbSet;

        public GenericRepository(ETailor_DBContext dBContext)
        {
            if (this.dBContext == null)
            {
                this.dBContext = dBContext;
            }
            dbSet = this.dBContext.Set<T>();
        }

        public async Task<bool> Create(T entity)
        {
            try
            {
                await dbSet.AddAsync(entity);
                dBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                var data = dbSet.Find(id);
                if (data == null)
                {
                    throw new UserException("Not found item: " + id);
                }
                else
                {
                    dbSet.Remove(data);
                    await dBContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public T Get(string id)
        {
            try
            {
                return dbSet.Find(id);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public IEnumerable<T> GetAll(Func<T, bool> where)
        {
            try
            {
                return dbSet.Where(where);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }

        public async Task<bool> Update(string id, T entity)
        {
            try
            {
                var data = dbSet.Find(id);
                if (data == null)
                {
                    throw new UserException("Not found item: " + id);
                }
                else
                {
                    dbSet.Update(entity);
                    await dBContext.SaveChangesAsync();
                    return true;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message);
            }
        }
    }
}
