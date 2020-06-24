using CustomORM.Interfaces;
using CustomORM.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM.Repository
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private DbManager db;

        public GenericRepository(string connectionString)
        {
            this.db = new DbManager(connectionString);
        }

        public void Create(TEntity item)
        {
            db.Add<TEntity>(item);
        }

        public void Delete(int id)
        {
            db.Remove<TEntity>(id.ToString());
        }

        public TEntity Get(int id)
        {
            return db.Find<TEntity>(id.ToString());
        }

        public IEnumerable<TEntity> GetAll()
        {
            return db.FindAll<TEntity>();
        }

        public void Update(TEntity item)
        {
            db.Update<TEntity>(item);
        }
    }
}
