using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletAPI.Services.Data.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _ctx;
        private readonly DbSet<T> entities;
        public int TotalNumberOfItems { get; set; }
        public GenericRepository(AppDbContext ctx)
        {
            _ctx = ctx;
            entities = ctx.Set<T>();
        }

        public async Task<bool> Delete(T entity)
        {
            entities.Remove(entity);
            return await _ctx.SaveChangesAsync() > 0;
        }

        //public async Task<T> Get(string id)
        //{
        //    return await entities.SingleOrDefaultAsync(s => s.Id == id);
        //}

        public async Task<IEnumerable<T>> GetAll()
        {
            var items = entities.AsEnumerable();
            TotalNumberOfItems = items.Count();
            return items;
        }

        public async Task<IEnumerable<T>> GetPaginated(int page, int per_page)
        {
            var items = await GetAll();
            var pagedItems = items.Skip((page - 1) * per_page).Take(per_page).ToList();
            return pagedItems;
        }

        public async Task<bool> Add(T entity)
        {
            await entities.AddAsync(entity);
            return await _ctx.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddRange(List<T> entity)
        {
            await entities.AddRangeAsync(entity);
            return await _ctx.SaveChangesAsync() > 0;
        }

        public async Task<bool> Update(T entity)
        {
            entities.Update(entity);
            return await _ctx.SaveChangesAsync() > 0;
        }
    }
}
