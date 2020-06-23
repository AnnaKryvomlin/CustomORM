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
    public class AuxiliaryShipRepository : IRepository<AuxiliaryShip>
    {
        private DbManager db;

        public AuxiliaryShipRepository(string connectionString)
        {
            this.db = new DbManager(connectionString);
        }

        public void Create(AuxiliaryShip item)
        {
            db.Add<AuxiliaryShip>(item);
        }

        public void Delete(int id)
        {
            db.Remove<AuxiliaryShip>(id.ToString());
        }

        public AuxiliaryShip Get(int id)
        {
            return db.Find<AuxiliaryShip>(id.ToString());
        }

        public IEnumerable<AuxiliaryShip> GetAll()
        {
            return db.FindAll<AuxiliaryShip>();
        }

        public void Update(AuxiliaryShip item)
        {
            db.Update(item);
        }
    }
}
