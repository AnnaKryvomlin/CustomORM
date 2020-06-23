using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM.Interfaces
{
    public interface IDbManager
    {
        IEnumerable<T> FindAll<T>();
        IEnumerable<T> FindAll<T>(string where);
        T Find<T>(string pk);
        T Find<T>(string pk, string where);
        void Add<T>(object entity);
        void Update(object entity);
        void Remove<T>(object entity);
        void Remove<T>(string pk);
    }
}
