using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace leave_management.Contracts
{
    //Set as global, all class can use, inherit this class
    public interface IRepositoryBase<T> where T : class
    {
        Task<ICollection<T>> FindAll(); //return all from db
        Task<T> FindById(int id);
        Task<bool> isExists(int id);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Save();
    }

    public interface IGenericRepository<T> where T : class
    {
        Task<IList<T>> FindAll(
                Expression<Func<T, bool>> expression = null, // lamda expression @ where condition {q => q.Id = Id}
                Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, // order the filter @ order by statement {q => q.OrderBy(q => q.Id)}
                List<string> includes = null // include statement {.Include(q => q.Employee)}
            );
        Task<T> Find(
                Expression<Func<T, bool>> expression, // lamda expression @ where condition {q => q.Id = Id}
                List<string> includes = null // include statement {.Include(q => q.Employee)}
            );
        Task<bool> isExists(
                Expression<Func<T, bool>> expression = null // lamda expression @ where condition {q => q.Id = Id}
            );
        Task Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        //Task<bool> Save();
    }
}
