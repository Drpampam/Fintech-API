using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WalletAPI.Services.Data.Services
{
    public interface IGenericRepository<T> where T : class
    {
        int TotalNumberOfItems { get; set; }
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetPaginated(int page, int per_page);
        //Task<T> Get(string id); 
        Task<bool> Add(T entity);
        Task<bool> AddRange(List<T> entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
    }
}
