using CustomORM.Repository;
using SeaFight.DLL.Entities;
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
            GenericRepository<Field> fieldRep = new GenericRepository<Field>(@"Data Source=AKINARU\SQLEXPRESS;Initial Catalog=SeaFight;Integrated Security=True");
            //Проверка cоздания
            fieldRep.Create(field);
            fieldRep.Create(field2);
            // Проверка функции Update
            field.ID = 1;
            field.ObjectsLimit = 5;
            fieldRep.Update(field);
            // Проверка удаления
            fieldRep.Delete(2);
            var fields = fieldRep.GetAll();
            foreach (var s in fields)
            {
                Console.WriteLine(s.GetType().Name + " " + s.ID + " " + s.Size + " " + s.XCenter + " " + s.YCenter + " " + s.ObjectsLimit);
            }

            Ship ship = new MilitaryShip(1, 3, 2, 1);
            GenericRepository<Ship> shipRep = new GenericRepository<Ship>(@"Data Source=AKINARU\SQLEXPRESS;Initial Catalog=SeaFight;Integrated Security=True");
            //Проверка cоздания
            shipRep.Create(ship);
            //Проверка получения по id
            Ship ship2 = shipRep.Get(4);
            Console.WriteLine(ship2.GetType().Name+ " " + ship2.Speed + " " + ship2.Length + " " + ship2.AbilityRange);
            // Проверка функции Update
            ship.ID = 1;
            shipRep.Update(ship);
            // Проверка удаления
            shipRep.Delete(3);
            var ships = shipRep.GetAll();
            // Вывести всё
            foreach (var s in ships)
            {
                Console.WriteLine(s.GetType().Name + " " + s.ID + " " + s.FieldID + " " + s.Speed + " " + s.Length + " " + s.AbilityRange);
                Console.WriteLine(s.Field.ObjectsLimit + " " + s.Field.Size);
            }
            Console.ReadKey();
        }
    }
}
