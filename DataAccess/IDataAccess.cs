using System.Collections.Generic;
using System.Threading.Tasks;

namespace kitchenview.DataAccess
{
    public interface IDataAccess<T>
    {
        public Task<IEnumerable<T>?> GetData();
    }
}