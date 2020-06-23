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

        public AuxiliaryShip(int fieldId, int length, int speed, int abilityRange, string name="") : base( fieldId, length, speed, abilityRange, name)
        {
            Name = "Военный";
        }

        public void RepairShip()
        {
            //TODO: Write something here
        }
    }
}
