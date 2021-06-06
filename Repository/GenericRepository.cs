using leave_management.Contracts;
using leave_management.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace leave_management.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _db;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task Create(T entity)
        {
            await _db.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _db.Remove(entity);
        }

        public async Task<T> Find(Expression<Func<T, bool>> expression, List<string> includes = null)
        {
            IQueryable<T> query = _db;

            if(includes != null)
            {
                foreach (var table in includes)
                {
                    query = query.Include(table);
                }
            }

            return await query.FirstOrDefaultAsync(expression);
        }

        public async Task<IList<T>> FindAll(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, List<string> includes = null)
        {
            IQueryable<T> query = _db;

            if(expression != null)
            {
                query = query.Where(expression);
            }

            if (includes != null)
            {
                foreach (var table in includes)
                {
                    query = query.Include(table);
                }
            }

            if(orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> isExists(Expression<Func<T, bool>> expression = null)
        {
            IQueryable<T> query = _db;
            return await query.AnyAsync(expression);
        }

        //public Task<bool> Save()
        //{
        //    throw new NotImplementedException();
        //}

        public void Update(T entity)
        {
            _db.Update(entity);
        }

        //Firstly, if your SP has parameters and won't return anything, then you code could look something like this.
        //    var name = new SqlParameter("@CategoryName", "Test");
        //        context.Database.ExecuteSqlCommand("EXEC AddCategory @CategoryName", name);


        //This could be put in a void function in the repository and context represents the existing context object. The parameter section is optional of course.
        //If you may need to return same thing from the SP call and have a matching Data Class(or table), then the code block would look something like:

        //var records = context.TABLE_NAME.FromSql($"SP_CALL {optional parameter}")
        //                      .ToList();
    }
}
