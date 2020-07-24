using SeaFight.DLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight.DLL.Entities
{
    public class AuxiliaryShip : Ship, IRepair
    {

        public AuxiliaryShip(int fieldId, int length, int speed, int abilityRange) : base( fieldId, length, speed, abilityRange)
        {}

        public AuxiliaryShip(int ID, int fieldID, int length, int speed, int abilityRange) : base(ID, fieldID, length, speed, abilityRange)
        { }

        public void RepairShip()
        {
            //TODO: Write something here
        }
    }
}
