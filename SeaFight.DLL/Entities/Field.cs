using CustomORM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight.DLL.Entities
{
    public class Field
    {
        [PK]
        public int ID { get; set; }
        [Ignore]
        public int[][] FieldSize { get; set; }
        [Ignore]
        public GridObject<Ship>[] ObjectsInField { get; private set; }
        public string Size { get; set; }
        public double XCenter { get; set; }
        public double YCenter { get; set; }
        public int ObjectsLimit { get; set; }
        public delegate void FieldHandler(string message);
        public event FieldHandler Notify;

        public Field(int objectsLimit = 10, string size = "", int x = 10, int y = 10)
        {
            FieldSize = new int[x][];
            this.Size = $"{x}x{y}";
            for (int i = 0; i < FieldSize.Length; i++)
            {
                FieldSize[i] = new int[y];
            }

            this.XCenter = x / 2;
            this.YCenter = y / 2;
            this.ObjectsInField = new GridObject<Ship>[objectsLimit];
            this.ObjectsLimit = objectsLimit;
        }

        public Field(int id, string size, double xCenter, double yCenter, int objectsLimit)
        {
            this.ID = ID;
            this.Size = size;
            this.XCenter = xCenter;
            this.YCenter = yCenter;
            this.ObjectsLimit = objectsLimit;
        }
    }
}
