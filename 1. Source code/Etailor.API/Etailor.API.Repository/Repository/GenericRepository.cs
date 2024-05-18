using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Ultity.CustomException;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public bool Create(T entity)
        {
            try
            {
                dbSet.Add(entity);
                dBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<bool> CreateAsync(T entity)
        {
            try
            {
                dbSet.Add(entity);
                await dBContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public bool CreateRange(List<T> entities)
        {
            try
            {
                dbSet.AddRange(entities);
                dBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<bool> CreateRangeAsync(List<T> entities)
        {
            try
            {
                dbSet.AddRange(entities);
                await dBContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public bool Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }
                else
                {
                    var data = dbSet.Find(id);
                    if (data == null)
                    {
                        throw new UserException("Not found item: " + id);
                    }
                    else
                    {
                        dbSet.Remove(data);
                        dBContext.SaveChanges();
                        return true;
                    }
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<bool> DeleteAsync(string? id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }
                else
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
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public T Get(string? id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }
                else
                {
                    return dbSet.Find(id);
                }
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<T> GetAsync(string? id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }
                else
                {
                    return await dbSet.FindAsync(id);
                }
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
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
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public IEnumerable<T> GetAllPagination(Func<T, bool> where, int? pageIndex, int? itemPerPage)
        {
            try
            {
                if (pageIndex == null || pageIndex < 1)
                {
                    pageIndex = 1;
                }
                if (itemPerPage == null || itemPerPage < 1)
                {
                    itemPerPage = 10;
                }
                // Calculate the number of items to skip based on pageIndex and itemPerPage
                int skipCount = (pageIndex.Value - 1) * itemPerPage.Value;

                // Use Skip and Take correctly to implement pagination
                return dbSet.Where(where).Skip(skipCount).Take(itemPerPage.Value);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }
        public int Count(Func<T, bool> where)
        {
            try
            {
                return dbSet.Where(where).Count();
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync(Func<T, bool> where)
        {
            try
            {
                Expression<Func<T, bool>> predicate = x => where(x);
                return await dbSet.Where(predicate).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public T FirstOrDefault(Func<T, bool> where)
        {
            try
            {
                return dbSet.FirstOrDefault(where);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public bool Update(string id, T entity)
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
                    dBContext.Entry(data).State = EntityState.Detached;
                    dbSet.Update(entity);
                    dBContext.SaveChanges();
                    return true;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<bool> UpdateAsync(string id, T entity)
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
                    dBContext.Entry(data).State = EntityState.Detached;
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
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public bool UpdateRange(List<T> entities)
        {
            try
            {
                dbSet.UpdateRange(entities);
                dBContext.SaveChanges();
                return true;
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task<bool> UpdateRangeAsync(List<T> entities)
        {
            try
            {
                dbSet.UpdateRange(entities);
                await dBContext.SaveChangesAsync();
                return true;
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public void Detach(string id)
        {
            try
            {
                var data = dbSet.Find(id);
                if (data != null)
                {
                    dBContext.Entry(data).State = EntityState.Detached;
                }
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public void SaveChange()
        {
            try
            {
                dBContext.SaveChanges();
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public async Task SaveChangeAsync()
        {
            try
            {
                await dBContext.SaveChangesAsync();
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }

        public IEnumerable<T> GetStoreProcedure(string storeProcedure, params SqlParameter[]? parameters)
        {
            try
            {
                if (parameters == null || parameters.Length == 0)
                {
                    return dbSet.FromSqlRaw(storeProcedure).AsNoTracking();
                }
                else
                {
                    var sqlQueryString = $"EXEC {storeProcedure} ";
                    foreach (var param in parameters)
                    {
                        sqlQueryString += $"{param.ParameterName}, ";
                    }

                    sqlQueryString = sqlQueryString.Remove(sqlQueryString.Length - 2);

                    return dbSet.FromSqlRaw(sqlQueryString, parameters).AsNoTracking();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetStoreProcedureReturnInt(string storeProcedure, params SqlParameter[]? parameters)
        {
            try
            {
                if (parameters == null || parameters.Length == 0)
                {
                    var result = await dBContext.Set<SpResult>().FromSqlRaw(storeProcedure)
                                           .AsNoTracking()
                                           .ToListAsync();

                    return result.FirstOrDefault().ReturnValue;
                }
                else
                {
                    var sqlQueryString = $"EXEC {storeProcedure} ";
                    foreach (var param in parameters)
                    {
                        sqlQueryString += $"{param.ParameterName}, ";
                    }

                    sqlQueryString = sqlQueryString.Remove(sqlQueryString.Length - 2);

                    var result = await dBContext.Set<SpResult>().FromSqlRaw(sqlQueryString, parameters)
                                           .AsNoTracking()
                                           .ToListAsync();

                    return result.FirstOrDefault().ReturnValue;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DatabaseFacade GetDatabase()
        {
            try
            {
                return dBContext.Database;
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }
        public ETailor_DBContext GetDbContext()
        {
            try
            {
                return dBContext;
            }
            catch (UserException ex)
            {
                throw new UserException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new SystemsException(ex.Message, nameof(T));
            }
        }
    }
}
