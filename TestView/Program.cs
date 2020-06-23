using SeaFight.DLL.Entities;
using SeaFight.DLL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestView
{
    class Program
    {
        static void Main(string[] args)
        {
            // Поле
            Field field = new Field();
            Field field2 = new Field(15, x: 10, y: 15);
            FieldRepository fieldRep = new FieldRepository(@"Data Source=AKINARU\SQLEXPRESS;Initial Catalog=SeaFight;Integrated Security=True");
            //Проверка cоздания
            fieldRep.Create(field);
            fieldRep.Create(field2);
            // Проверка функции Update
            field.ID = 1;
            field.ObjectsLimit = 5;
            fieldRep.Update(field);
            // Проверка удаления
            fieldRep.Delete(2);

            AuxiliaryShip ship = new AuxiliaryShip(1, 2, 2, 1);
            AuxiliaryShipRepository shipRep = new AuxiliaryShipRepository(@"Data Source=AKINARU\SQLEXPRESS;Initial Catalog=SeaFight;Integrated Security=True");
            //Проверка cоздания
            shipRep.Create(ship);
            //Проверка получения по id
            AuxiliaryShip ship2 = shipRep.Get(4);
            Console.WriteLine(ship2.Name + " " + ship2.Speed + " " + ship2.Length + " " + ship2.AbilityRange);
            // Проверка функции Update
            ship.ID = 1;
            shipRep.Update(ship);
            // Проверка удаления
            shipRep.Delete(3);
            // Вывести всё
            var ships = shipRep.GetAll();
            foreach(var s in ships)
            {
                Console.WriteLine(s.Name + " " + s.Speed + " " + s.Length + " " + s.AbilityRange);
            }

            Console.ReadKey();
        }
    }
}
