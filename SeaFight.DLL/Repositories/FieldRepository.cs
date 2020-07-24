using CustomORM.Interfaces;
using CustomORM.ORM;
using SeaFight.DLL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight.DLL.Repositories
{
    public class FieldRepository : IRepository<Field>
    {
        private DbManager db;

        public FieldRepository(string connectionString)
        {
            this.db = new DbManager(connectionString);
        }

        public void Create(Field item)
        {
            db.Add<Field>(item);
        }

        public void Delete(int id)
        {
            db.Remove<Field>(id.ToString());
        }

        public Field Get(int id)
        {
            return db.Find<Field>(id.ToString());
        }

        public IEnumerable<Field> GetAll()
        {
            return db.FindAll<Field>();
        }

        public void Update(Field item)
        {
            db.Update(item);
        }
    }
}
