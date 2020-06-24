using SeaFight.DLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight.DLL.Entities
{
    public class MixedTypeShip : Ship, IRepair, IShot
    {

        public MixedTypeShip(int fieldId, int length, int speed, int abilityRange) : base(fieldId, length, speed, abilityRange)
        { }

        public MixedTypeShip(int ID, int fieldID, int length, int speed, int abilityRange) : base(ID, fieldID, length, speed, abilityRange)
        { }

        public void RepairShip()
        {
            //TODO: Write something here
        }

        public void ShootTheEnemy()
        {
            //TODO: Write something here
        }
    }
}
