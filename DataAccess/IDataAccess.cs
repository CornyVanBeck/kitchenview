using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace kitchenview.DataAccess
{
    public interface IDataAccess<T>
    {
        public Task<IEnumerable<T>> GetData();
    }
}